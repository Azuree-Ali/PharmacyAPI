using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmacyAPI.Models;
using PharmacyAPI.Repositories;
using PharmacyAPI.Services;
using PharmacyAPI.Utils;

namespace PharmacyAPI.Areas.Customer.Controllers
{
    [Authorize]
    [Area(CD.CUSTOMER_AREA)]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public sealed class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IRepository<Notification> _notificationRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsController(
            INotificationService notificationService,
            IRepository<Notification> notificationRepository,
            UserManager<ApplicationUser> userManager)
        {
            _notificationService = notificationService;
            _notificationRepository = notificationRepository;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetMine()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized(new ApiResponse<object> { IsSuccess = false, Message = "User is not authenticated." });

            var notifications = await _notificationService.GetUserNotificationsAsync(userId);
            return Ok(new ApiResponse<object>
            {
                IsSuccess = true,
                Message = "Notifications retrieved successfully.",
                Data = notifications.Select(ToResponse).ToList()
            });
        }

        [HttpPut("{id:int}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized(new ApiResponse<object> { IsSuccess = false, Message = "User is not authenticated." });

            var notification = await _notificationRepository.GetOneAsync(
                filter: n => n.Id == id && n.UserId == userId,
                IsTracking: false);
            if (notification == null)
                return NotFound(new ApiResponse<object> { IsSuccess = false, Message = "Notification not found." });

            await _notificationService.MarkAsReadAsync(id, userId);
            return Ok(new ApiResponse<object> { IsSuccess = true, Message = "Notification marked as read." });
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized(new ApiResponse<object> { IsSuccess = false, Message = "User is not authenticated." });

            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(new ApiResponse<object> { IsSuccess = true, Message = "All notifications marked as read." });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized(new ApiResponse<object> { IsSuccess = false, Message = "User is not authenticated." });

            var notification = await _notificationRepository.GetOneAsync(
                filter: n => n.Id == id && n.UserId == userId);
            if (notification == null)
                return NotFound(new ApiResponse<object> { IsSuccess = false, Message = "Notification not found." });

            _notificationRepository.Delete(notification);
            await _notificationRepository.CommitAsync();
            return Ok(new ApiResponse<object> { IsSuccess = true, Message = "Notification deleted successfully." });
        }

        private static object ToResponse(Notification notification) => new
        {
            notification.Id,
            notification.Message,
            notification.Type,
            notification.OrderId,
            notification.IsRead,
            notification.CreatedAt
        };
    }
}
