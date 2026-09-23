using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PharmacyAPI.DataAccess;
using PharmacyAPI.DTOs.Request;
using PharmacyAPI.Enums;
using PharmacyAPI.Hubs;
using PharmacyAPI.Models;
using PharmacyAPI.Utils;

namespace PharmacyAPI.Services
{
    public sealed class OrderWorkflowService : IOrderWorkflowService
    {
        private const string DeliveryPrompt = "Hello sir is the order arrived";
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _chatHub;
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly ILogger<OrderWorkflowService> _logger;

        public OrderWorkflowService(
            ApplicationDbContext context,
            IHubContext<ChatHub> chatHub,
            IHubContext<NotificationHub> notificationHub,
            ILogger<OrderWorkflowService> logger)
        {
            _context = context;
            _chatHub = chatHub;
            _notificationHub = notificationHub;
            _logger = logger;
        }

        public async Task<Order> CheckoutAsync(
            string customerId,
            CheckoutRequest request)
        {
            if (request.PaymentMethod == PaymentMethod.CreditCard)
            {
                throw new InvalidOperationException("Credit card checkout is not handled by this service.");
            }

            if (request.PaymentMethod != PaymentMethod.Cash)
            {
                throw new InvalidOperationException("Unsupported payment method.");
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.ApplicationUserId == customerId);

            if (cart == null || cart.CartItems.Count == 0)
            {
                throw new InvalidOperationException("Your cart is empty.");
            }

            var lines = new List<CheckoutLine>();
            foreach (var cartItem in cart.CartItems)
            {
                var product = await _context.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == cartItem.ProductId);

                if (product == null)
                {
                    throw new InvalidOperationException($"Product with ID {cartItem.ProductId} was not found.");
                }

                var batches = await _context.ProductBatches
                    .Where(b => b.ProductId == product.Id
                        && b.QuantityOnHand > 0
                        && b.ExpiryDate >= DateTime.UtcNow.Date)
                    .OrderBy(b => b.ExpiryDate)
                    .ToListAsync();

                if (batches.Sum(b => b.QuantityOnHand) < cartItem.Quantity)
                {
                    throw new InvalidOperationException($"Insufficient stock for product '{product.Name}'.");
                }

                lines.Add(new CheckoutLine(cartItem, product, batches));
            }

            var total = lines.Sum(line => line.CartItem.Quantity * line.Product.Price);
            var order = new Order
            {
                OrderNumber = $"ORD-{Guid.NewGuid():N}",
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                IsPaid = false,
                TotalAmount = total,
                Discount = 0,
                DeliveryFees = 0,
                NetAmount = total,
                PaymentMethod = PaymentMethod.Cash,
                DeliveryAddress = request.DeliveryAddress,
                Notes = request.Notes,
                ApplicationUserId = customerId
            };

            foreach (var line in lines)
            {
                var orderItem = new OrderItem
                {
                    ProductId = line.Product.Id,
                    Quantity = line.CartItem.Quantity,
                    UnitPrice = line.Product.Price,
                    TotalPrice = line.CartItem.Quantity * line.Product.Price
                };

                var quantityRemaining = line.CartItem.Quantity;
                foreach (var batch in line.Batches)
                {
                    if (quantityRemaining == 0)
                    {
                        break;
                    }

                    var allocated = Math.Min(batch.QuantityOnHand, quantityRemaining);
                    batch.QuantityOnHand -= allocated;
                    quantityRemaining -= allocated;
                    orderItem.BatchAllocations.Add(new OrderItemBatchAllocation
                    {
                        ProductBatchId = batch.Id,
                        Quantity = allocated
                    });
                }

                order.OrderItems.Add(orderItem);
            }

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<Order?> ConfirmArrivalAsync(
            string customerId,
            int orderId)
        {
            var order = await GetCustomerCashOrderAsync(customerId, orderId);
            if (order == null)
            {
                return null;
            }

            if (order.Status != OrderStatus.Pending || order.IsPaid
                || order.DeliveryConfirmationRequestedAt == null)
            {
                return null;
            }

            order.Status = OrderStatus.Completed;
            order.IsPaid = true;
            await DisableDeliveryActionsAsync(order.Id);
            var chatMessage = await CreateOrderChatMessageAsync(
                order,
                "Thank you for confirming. Your order is completed and payment is marked as paid.");
            await _context.SaveChangesAsync();
            if (chatMessage != null)
            {
                await BroadcastChatMessageAsync(chatMessage);
            }

            return order;
        }

        public async Task<Order?> CancelAsync(
            string customerId,
            int orderId)
        {
            var order = await GetCustomerCashOrderAsync(customerId, orderId);
            if (order == null)
            {
                return null;
            }

            if (order.Status != OrderStatus.Pending
                || order.IsPaid
                || order.DeliveryConfirmationRequestedAt == null)
            {
                return null;
            }

            if (order.OrderItems.Any(item =>
                    item.BatchAllocations.Sum(allocation => allocation.Quantity) != item.Quantity))
            {
                return null;
            }

            foreach (var allocation in order.OrderItems
                         .SelectMany(item => item.BatchAllocations))
            {
                if (allocation.ProductBatch == null)
                {
                    throw new InvalidOperationException(
                        $"Inventory batch {allocation.ProductBatchId} was not found.");
                }

                allocation.ProductBatch.QuantityOnHand += allocation.Quantity;
            }

            order.Status = OrderStatus.Cancelled;
            await DisableDeliveryActionsAsync(order.Id);
            var chatMessage = await CreateOrderChatMessageAsync(
                order,
                "Your order was cancelled and the reserved stock has been restored.");
            await _context.SaveChangesAsync();
            if (chatMessage != null)
            {
                await BroadcastChatMessageAsync(chatMessage);
            }

            return order;
        }

        public async Task HandleCustomerChatReplyAsync(
            int chatId,
            string customerId,
            string message)
        {
            var normalized = message.Trim().Trim('.', '!', '?').ToLowerInvariant();
            if (normalized is not ("no" or "not yet" or "not arrived"))
            {
                return;
            }

            var chat = await _context.Chats.FirstOrDefaultAsync(
                c => c.Id == chatId && c.CustomerId == customerId);
            if (chat?.AdminId == null)
            {
                return;
            }

            var order = await _context.Orders
                .Where(o => o.ApplicationUserId == customerId
                    && o.PaymentMethod == PaymentMethod.Cash
                    && o.Status == OrderStatus.Pending
                    && !o.IsPaid
                    && o.DeliveryConfirmationRequestedAt != null)
                .OrderByDescending(o => o.DeliveryConfirmationRequestedAt)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return;
            }

            var reply = new ChatMessage
            {
                ChatId = chat.Id,
                SenderId = chat.AdminId,
                OrderId = order.Id,
                Message = "Your order will arrive soon.",
                SentAt = DateTime.UtcNow
            };

            _context.ChatMessages.Add(reply);
            await _context.SaveChangesAsync();
            await BroadcastChatMessageAsync(reply);
        }

        public async Task DispatchDueDeliveryPromptsAsync(
            CancellationToken cancellationToken)
        {
            var cutoff = DateTime.UtcNow.AddMinutes(-30);
            var dueOrderIds = await _context.Orders
                .Where(order => order.PaymentMethod == PaymentMethod.Cash
                    && order.Status == OrderStatus.Pending
                    && !order.IsPaid
                    && order.DeliveryConfirmationRequestedAt == null
                    && order.OrderDate <= cutoff
                    && order.OrderItems.Any(item => item.BatchAllocations.Any()))
                .OrderBy(order => order.OrderDate)
                .Select(order => order.Id)
                .Take(50)
                .ToListAsync(cancellationToken);

            foreach (var orderId in dueOrderIds)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await DispatchPromptForOrderAsync(orderId, cancellationToken);
            }
        }

        private async Task DispatchPromptForOrderAsync(
            int orderId,
            CancellationToken cancellationToken)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o =>
                o.Id == orderId
                && o.PaymentMethod == PaymentMethod.Cash
                && o.Status == OrderStatus.Pending
                && !o.IsPaid
                && o.DeliveryConfirmationRequestedAt == null
                && o.OrderDate <= DateTime.UtcNow.AddMinutes(-30), cancellationToken);

            if (order == null)
            {
                return;
            }

            var chat = await _context.Chats.FirstOrDefaultAsync(
                c => c.CustomerId == order.ApplicationUserId,
                cancellationToken);

            var adminId = chat?.AdminId ?? await GetSupportAdminIdAsync(cancellationToken);
            if (adminId == null)
            {
                _logger.LogWarning(
                    "No support administrator is available to send delivery prompt for order {OrderId}.",
                    order.Id);
                return;
            }

            if (chat == null)
            {
                chat = new Chat
                {
                    CustomerId = order.ApplicationUserId,
                    AdminId = adminId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Chats.Add(chat);
            }
            else if (chat.AdminId == null)
            {
                chat.AdminId = adminId;
            }

            var now = DateTime.UtcNow;
            var chatMessage = new ChatMessage
            {
                Chat = chat,
                SenderId = adminId,
                OrderId = order.Id,
                Message = DeliveryPrompt,
                SentAt = now,
                OffersDeliveryActions = true
            };

            var notification = new Notification
            {
                UserId = order.ApplicationUserId,
                Message = "Please confirm whether your order has arrived.",
                Type = "OrderDelivery",
                OrderId = order.Id,
                CreatedAt = now
            };

            order.DeliveryConfirmationRequestedAt = now;
            _context.ChatMessages.Add(chatMessage);
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync(cancellationToken);
            await BroadcastChatMessageAsync(chatMessage);
            await _notificationHub.Clients.User(order.ApplicationUserId)
                .SendAsync("ReceiveNotification", new
                {
                    notification.Id,
                    notification.Message,
                    notification.Type,
                    notification.OrderId,
                    notification.IsRead,
                    notification.CreatedAt
                }, cancellationToken);
        }

        private Task<Order?> GetCustomerCashOrderAsync(string customerId, int orderId) =>
            _context.Orders
                .Include(order => order.OrderItems)
                    .ThenInclude(item => item.BatchAllocations)
                        .ThenInclude(allocation => allocation.ProductBatch)
                .FirstOrDefaultAsync(order => order.Id == orderId
                    && order.ApplicationUserId == customerId
                    && order.PaymentMethod == PaymentMethod.Cash);

        private async Task<ChatMessage?> CreateOrderChatMessageAsync(
            Order order,
            string message)
        {
            var chat = await _context.Chats.FirstOrDefaultAsync(
                item => item.CustomerId == order.ApplicationUserId);
            if (chat?.AdminId == null)
            {
                return null;
            }

            var chatMessage = new ChatMessage
            {
                ChatId = chat.Id,
                SenderId = chat.AdminId,
                OrderId = order.Id,
                Message = message,
                SentAt = DateTime.UtcNow
            };
            _context.ChatMessages.Add(chatMessage);
            return chatMessage;
        }

        private async Task DisableDeliveryActionsAsync(int orderId)
        {
            var promptMessages = await _context.ChatMessages
                .Where(message => message.OrderId == orderId
                    && message.OffersDeliveryActions)
                .ToListAsync();

            foreach (var promptMessage in promptMessages)
            {
                promptMessage.OffersDeliveryActions = false;
            }
        }

        private async Task<string?> GetSupportAdminIdAsync(CancellationToken cancellationToken)
        {
            var supportRoles = new[]
            {
                CD.SUPER_ADMIN_ROLE,
                CD.ADMIN_ROLE,
                CD.PHARMACIST_ROLE
            };

            return await (from userRole in _context.UserRoles
                          join role in _context.Roles on userRole.RoleId equals role.Id
                          where role.Name != null && supportRoles.Contains(role.Name)
                          orderby role.Name
                          select userRole.UserId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private Task BroadcastChatMessageAsync(ChatMessage message) =>
            _chatHub.Clients.Group($"chat-{message.ChatId}")
                .SendAsync("ReceiveMessage", new
                {
                    id = message.Id,
                    chatId = message.ChatId,
                    senderId = message.SenderId,
                    message = message.Message,
                    sentAt = message.SentAt,
                    isRead = message.IsRead,
                    orderId = message.OrderId,
                    actions = GetChatActions(message.OffersDeliveryActions ? message.OrderId : null)
                });

        internal static object[] GetChatActions(int? orderId) => orderId.HasValue
            ? new object[]
            {
                new { label = "Arrived", method = "POST", href = $"/api/Customer/Orders/{orderId}/arrived" },
                new { label = "Cancel", method = "POST", href = $"/api/Customer/Orders/{orderId}/cancel" }
            }
            : Array.Empty<object>();

        private sealed record CheckoutLine(
            CartItem CartItem,
            Product Product,
            List<ProductBatch> Batches);
    }
}
