using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyAPI.DTOs.Request;
using PharmacyAPI.DTOs.Response;
using PharmacyAPI.Enums;
using PharmacyAPI.Models;
using PharmacyAPI.Repositories;
using PharmacyAPI.Utils;

namespace PharmacyAPI.Areas.Customer.Controllers
{
    [Authorize]
    [Area(CD.CUSTOMER_AREA)]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class CheckoutController : ControllerBase
    {
        private readonly IRepository<Cart> _cartRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<ProductBatch> _productBatchRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutController(
            IRepository<Cart> cartRepository,
            IRepository<Product> productRepository,
            IRepository<Order> orderRepository,
            IRepository<OrderItem> orderItemRepository,
            IRepository<ProductBatch> productBatchRepository,
            UserManager<ApplicationUser> userManager)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productBatchRepository = productBatchRepository;
            _userManager = userManager;
        }

        // POST: api/Customer/Checkout
        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutRequest request)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "User is not authenticated."
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Invalid checkout data.",
                    Data = ModelState
                });
            }

            var cart = await _cartRepository.GetOneAsync(
                filter: c => c.ApplicationUserId == userId,
                includes: [c => c.CartItems]
            );

            if (cart == null || !cart.CartItems.Any())
            {
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Your cart is empty."
                });
            }

            // --------------------------------------------------
            // CREDIT CARD
            // --------------------------------------------------

            if (request.PaymentMethod == PaymentMethod.CreditCard)
            {
                // Payment gateway will be connected here.
                // Do NOT create the order before payment succeeds.

                return Ok(new ApiResponse<object>
                {
                    IsSuccess = true,
                    Message = "Continue to payment."
                });
            }

            // --------------------------------------------------
            // CASH
            // --------------------------------------------------

            foreach (var cartItem in cart.CartItems)
            {
                var product = await _productRepository.GetOneAsync(
                    filter: p => p.Id == cartItem.ProductId,
                    IsTracking: false
                );

                if (product == null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = $"Product with ID {cartItem.ProductId} was not found."
                    });
                }

                var batches = await _productBatchRepository.GetAllAsync(
                    filter: b =>
                        b.ProductId == cartItem.ProductId &&
                        b.QuantityOnHand > 0 &&
                        b.ExpiryDate > DateTime.Now,
                    IsTracking: false
                );

                var availableQuantity = batches.Sum(b => b.QuantityOnHand);

                if (availableQuantity < cartItem.Quantity)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = $"Insufficient stock for product '{product.Name}'."
                    });
                }
            }

            // --------------------------------------------------
            // CREATE ORDER
            // --------------------------------------------------

            var totalAmount = cart.CartItems.Sum(x => x.TotalPrice);

            var order = new Order
            {
                OrderNumber = $"ORD-{DateTime.Now:yyyyMMddHHmmssfff}",
                OrderDate = DateTime.Now,
                Status = OrderStatus.Pending,
                TotalAmount = totalAmount,
                Discount = 0,
                DeliveryFees = 0,
                NetAmount = totalAmount,
                PaymentMethod = PaymentMethod.Cash,
                DeliveryAddress = request.DeliveryAddress,
                Notes = request.Notes,
                ApplicationUserId = userId
            };

            await _orderRepository.CreateAsync(order);
            await _orderRepository.CommitAsync();

            // --------------------------------------------------
            // CREATE ORDER ITEMS + DEDUCT STOCK
            // --------------------------------------------------

            foreach (var cartItem in cart.CartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice,
                    TotalPrice = cartItem.TotalPrice
                };

                await _orderItemRepository.CreateAsync(orderItem);

                var batches = (await _productBatchRepository.GetAllAsync(
                    filter: b =>
                        b.ProductId == cartItem.ProductId &&
                        b.QuantityOnHand > 0 &&
                        b.ExpiryDate > DateTime.Now,
                    IsTracking: true
                ))
                .OrderBy(b => b.ExpiryDate)
                .ToList();

                var remainingQuantity = cartItem.Quantity;

                foreach (var batch in batches)
                {
                    if (remainingQuantity <= 0)
                        break;

                    var quantityToTake = Math.Min(
                        batch.QuantityOnHand,
                        remainingQuantity
                    );

                    batch.QuantityOnHand -= quantityToTake;
                    remainingQuantity -= quantityToTake;

                    _productBatchRepository.Update(batch);
                }
            }

            // --------------------------------------------------
            // CLEAR CART
            // --------------------------------------------------

            foreach (var cartItem in cart.CartItems.ToList())
            {
                // We need the CartItem repository here.
            }

            await _orderRepository.CommitAsync();

            return Ok(new ApiResponse<object>
            {
                IsSuccess = true,
                Message = "Order created successfully.",
                Data = new
                {
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    TotalAmount = order.TotalAmount,
                    NetAmount = order.NetAmount
                }
            });
        }
    }
}