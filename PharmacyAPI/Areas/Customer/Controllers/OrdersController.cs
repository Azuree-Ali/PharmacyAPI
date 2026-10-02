using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmacyAPI.DTOs.Response;
using PharmacyAPI.Models;
using PharmacyAPI.Services;
using PharmacyAPI.Utils;

namespace PharmacyAPI.Areas.Customer.Controllers
{
    [Authorize(Roles = CD.CUSTOMER_ROLE)]
    [Area(CD.CUSTOMER_AREA)]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public sealed class OrdersController : ControllerBase
    {
        private readonly IOrderWorkflowService _orderWorkflowService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(
            IOrderWorkflowService orderWorkflowService,
            UserManager<ApplicationUser> userManager)
        {
            _orderWorkflowService = orderWorkflowService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderHistory()
        {
            var customerId = _userManager.GetUserId(User);
            if (customerId == null)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "User is not authenticated."
                });
            }

            var orders = await _orderWorkflowService.GetCustomerOrderHistoryAsync(customerId);
            var response = orders.Select(order => new OrderResponse
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
                Notes = order.Notes
            }).ToList();

            return Ok(new ApiResponse<List<OrderResponse>>
            {
                IsSuccess = true,
                Message = "Order history retrieved successfully.",
                Data = response
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrderDetails(int id)
        {
            var customerId = _userManager.GetUserId(User);
            if (customerId == null)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "User is not authenticated."
                });
            }

            var order = await _orderWorkflowService.GetCustomerOrderDetailsAsync(customerId, id);
            if (order == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Order not found."
                });
            }

            var response = new OrderDetailsResponse
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                DeliveryConfirmationRequestedAt = order.DeliveryConfirmationRequestedAt,
                Status = order.Status,
                IsPaid = order.IsPaid,
                TotalAmount = order.TotalAmount,
                Discount = order.Discount,
                DeliveryFees = order.DeliveryFees,
                NetAmount = order.NetAmount,
                PaymentMethod = order.PaymentMethod,
                DeliveryAddress = order.DeliveryAddress,
                Notes = order.Notes,
                OrderItems = order.OrderItems.Select(item => new OrderItemResponse
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Product?.Name,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                }).ToList()
            };

            return Ok(new ApiResponse<OrderDetailsResponse>
            {
                IsSuccess = true,
                Message = "Order details retrieved successfully.",
                Data = response
            });
        }

        [HttpPost("{id:int}/arrived")]
        public async Task<IActionResult> ConfirmArrival(int id)
        {
            var customerId = _userManager.GetUserId(User);
            if (customerId == null)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "User is not authenticated."
                });
            }

            var order = await _orderWorkflowService.ConfirmArrivalAsync(customerId, id);
            if (order == null)
            {
                return Conflict(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Order was not found or is not awaiting delivery confirmation."
                });
            }

            return Ok(new ApiResponse<object>
            {
                IsSuccess = true,
                Message = "Delivery confirmed. The order is completed and payment is marked as paid.",
                Data = new { order.Id, order.Status, order.IsPaid }
            });
        }

        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var customerId = _userManager.GetUserId(User);
            if (customerId == null)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "User is not authenticated."
                });
            }

            var order = await _orderWorkflowService.CancelAsync(customerId, id);
            if (order == null)
            {
                return Conflict(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Order was not found or is not eligible for cancellation."
                });
            }

            return Ok(new ApiResponse<object>
            {
                IsSuccess = true,
                Message = "Order cancelled and reserved inventory restored.",
                Data = new { order.Id, order.Status, order.IsPaid }
            });
        }
    }
}
