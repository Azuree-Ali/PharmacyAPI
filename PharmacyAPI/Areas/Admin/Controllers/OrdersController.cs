using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PharmacyAPI.Models;
using PharmacyAPI.Repositories;
using PharmacyAPI.Services;
using PharmacyAPI.Utils;

namespace PharmacyAPI.Areas.Admin.Controllers
{
    [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE},{CD.PHARMACIST_ROLE}")]
    [Area(CD.ADMIN_AREA)]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _itemRepository;
        private readonly IOrderWorkflowService _orderWorkflowService;
        private readonly INotificationService _notificationService;


        public OrdersController(
            IRepository<Order> orderRepository,
            IRepository<OrderItem> itemRepository,
            IOrderWorkflowService orderWorkflowService,
            INotificationService notificationService)
        {
            _orderRepository = orderRepository;
            _itemRepository = itemRepository;
            _orderWorkflowService = orderWorkflowService;
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _orderRepository.GetAllAsync(
                includes:
                [
                    o => o.ApplicationUser
                ]
            );

            var response = orders
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderResponse
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    IsPaid = o.IsPaid,
                    TotalAmount = o.TotalAmount,
                    Discount = o.Discount,
                    DeliveryFees = o.DeliveryFees,
                    NetAmount = o.NetAmount,
                    PaymentMethod = o.PaymentMethod,
                    DeliveryAddress = o.DeliveryAddress,
                    Notes = o.Notes,
                    ApplicationUserId = o.ApplicationUserId
                })
                .ToList();

            return Ok(new ApiResponse<List<OrderResponse>>
            {
                IsSuccess = true,
                Message = "Orders retrieved successfully",
                Data = response
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _orderRepository.GetOneAsync(
                filter: o => o.Id == id,
                includes:
                [
                    o => o.ApplicationUser,
            o => o.OrderItems
                ]
            );

            if (order == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Order not found"
                });
            }

            var response = new OrderDetailsResponse
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                Status = order.Status,
                IsPaid = order.IsPaid,
                TotalAmount = order.TotalAmount,
                Discount = order.Discount,
                DeliveryFees = order.DeliveryFees,
                NetAmount = order.NetAmount,
                PaymentMethod = order.PaymentMethod,
                DeliveryAddress = order.DeliveryAddress,
                Notes = order.Notes,
                ApplicationUserId = order.ApplicationUserId,

                OrderItems = order.OrderItems.Select(item => new OrderItemResponse
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                }).ToList()
            };

            return Ok(new ApiResponse<OrderDetailsResponse>
            {
                IsSuccess = true,
                Message = "Order retrieved successfully",
                Data = response
            });
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateOrderRequest request)
        {
            Order order;
            try
            {
                order = await _orderWorkflowService.CreateAdminOrderAsync(request);
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = exception.Message
                });
            }

            var response = new OrderDetailsResponse
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                Status = order.Status,
                IsPaid = order.IsPaid,
                TotalAmount = order.TotalAmount,
                Discount = order.Discount,
                DeliveryFees = order.DeliveryFees,
                NetAmount = order.NetAmount,
                PaymentMethod = order.PaymentMethod,
                DeliveryAddress = order.DeliveryAddress,
                Notes = order.Notes,
                ApplicationUserId = order.ApplicationUserId,

                OrderItems = order.OrderItems.Select(item => new OrderItemResponse
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                }).ToList()
            };

            return Ok(new ApiResponse<OrderDetailsResponse>
            {
                IsSuccess = true,
                Message = "Order created successfully",
                Data = response
            });
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateOrderRequest request)
        {
            var order = await _orderRepository.GetOneAsync(
                filter: o => o.Id == id,
                includes:
                [
                    o => o.OrderItems
                ]
            );

            if (order == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Order not found"
                });
            }

            if (!Enum.IsDefined(typeof(Enums.OrderStatus), request.Status))
            {
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Invalid order status."
                });
            }

            if (request.PaymentMethod.HasValue
                && (!Enum.IsDefined(typeof(Enums.PaymentMethod), request.PaymentMethod.Value)
                    || request.PaymentMethod.Value != order.PaymentMethod))
            {
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Payment method cannot be changed after order creation."
                });
            }

            if (!string.IsNullOrWhiteSpace(request.ApplicationUserId)
                && request.ApplicationUserId != order.ApplicationUserId)
            {
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "An order cannot be reassigned to another customer."
                });
            }

            if (request.Discount < 0 || request.DeliveryFees < 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Discount and delivery fees cannot be negative."
                });
            }

            if (request.OrderItems != null
                && (request.OrderItems.Count != order.OrderItems.Count
                    || request.OrderItems.Any(requestItem =>
                        !order.OrderItems.Any(orderItem =>
                            orderItem.Id == requestItem.Id
                            && orderItem.ProductId == requestItem.ProductId
                            && orderItem.Quantity == requestItem.Quantity))))
            {
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Order items cannot be edited here because their stock allocations must remain accurate. Cancel the order through the delivery flow and create a new order instead."
                });
            }

            if (request.Discount > order.TotalAmount)
            {
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Discount cannot exceed the order total."
                });
            }

            if (request.Status != order.Status)
            {
                if (order.Status is Enums.OrderStatus.Completed or Enums.OrderStatus.Cancelled)
                {
                    return Conflict(new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = "Completed and cancelled orders cannot be reopened or changed."
                    });
                }

                if (request.Status == Enums.OrderStatus.Cancelled)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = "Cancel orders through the customer delivery confirmation flow so reserved stock is restored."
                    });
                }

                if ((request.Status is Enums.OrderStatus.Processing or Enums.OrderStatus.Completed)
                    && !order.IsPaid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = "An unpaid order cannot be moved to processing or completed."
                    });
                }
            }

            var statusChanged = order.Status != request.Status;
            order.Status = request.Status;
            order.Discount = request.Discount;
            order.DeliveryFees = request.DeliveryFees;
            order.DeliveryAddress = request.DeliveryAddress;
            order.TotalAmount = order.OrderItems.Sum(item => item.TotalPrice);
            order.NetAmount = order.TotalAmount - order.Discount + order.DeliveryFees;

            await _orderRepository.CommitAsync();

            if (statusChanged)
            {
                await _notificationService.CreateAsync(
                    order.ApplicationUserId,
                    $"Your order {order.OrderNumber} status changed to {order.Status}.",
                    "OrderStatus",
                    order.Id);
            }

            var response = new OrderDetailsResponse
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                Status = order.Status,
                IsPaid = order.IsPaid,
                TotalAmount = order.TotalAmount,
                Discount = order.Discount,
                DeliveryFees = order.DeliveryFees,
                NetAmount = order.NetAmount,
                PaymentMethod = order.PaymentMethod,
                DeliveryAddress = order.DeliveryAddress,
                Notes = order.Notes,
                ApplicationUserId = order.ApplicationUserId,

                OrderItems = order.OrderItems
                    .Select(item => new OrderItemResponse
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice
                    })
                    .ToList()
            };

            return Ok(new ApiResponse<OrderDetailsResponse>
            {
                IsSuccess = true,
                Message = "Order updated successfully",
                Data = response
            });
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _orderRepository.GetOneAsync(
                filter: o => o.Id == id,
                includes:
                [
                    o => o.OrderItems,
            o => o.Notifications
                ]
            );

            if (order == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Order not found"
                });
            }

            if (order.PaymentMethod == Enums.PaymentMethod.Cash
                && order.Status == Enums.OrderStatus.Pending
                && !order.IsPaid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Cancel this cash order through the customer delivery flow before deleting it so reserved inventory can be restored."
                });
            }

            if (order.Status is Enums.OrderStatus.Processing or Enums.OrderStatus.Completed)
            {
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Processing and completed order records cannot be deleted. Preserve them for order history and audit."
                });
            }
            // Delete order items
            foreach (var item in order.OrderItems.ToList())
            {
                _itemRepository.Delete(item);
            }

            // Delete order
            _orderRepository.Delete(order);

            await _orderRepository.CommitAsync();

            return Ok(new ApiResponse<object>
            {
                IsSuccess = true,
                Message = "Order deleted successfully"
            });
        }
    }
}
