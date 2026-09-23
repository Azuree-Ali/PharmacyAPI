using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmacyAPI.DTOs.Request;
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
    public class CheckoutController : ControllerBase
    {
        private readonly IOrderWorkflowService _orderWorkflowService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutController(
            IOrderWorkflowService orderWorkflowService,
            UserManager<ApplicationUser> userManager)
        {
            _orderWorkflowService = orderWorkflowService;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutRequest request)
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

            try
            {
                var order = await _orderWorkflowService.CheckoutAsync(customerId, request);
                return Ok(new ApiResponse<object>
                {
                    IsSuccess = true,
                    Message = "Cash order created. Payment is due when delivery is confirmed.",
                    Data = new
                    {
                        order.Id,
                        order.OrderNumber,
                        order.TotalAmount,
                        order.NetAmount,
                        order.Status,
                        order.IsPaid
                    }
                });
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = exception.Message
                });
            }
        }
    }
}
