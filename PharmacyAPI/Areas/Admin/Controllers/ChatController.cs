using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmacyAPI.DTOs.Response;
using PharmacyAPI.Models;
using PharmacyAPI.Services;
using PharmacyAPI.Utils;

namespace PharmacyAPI.Areas.Admin.Controllers
{
    [Authorize(Roles =
        $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE},{CD.PHARMACIST_ROLE}")]
    [Area(CD.ADMIN_AREA)]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatController(
            IChatService chatService,
            UserManager<ApplicationUser> userManager)
        {
            _chatService = chatService;
            _userManager = userManager;
        }

        // GET: api/Admin/Chats
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var adminId = _userManager.GetUserId(User);

            if (adminId == null)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "User is not authenticated."
                });
            }

            var chats = await _chatService.GetAdminChatsAsync(adminId);

            var response = chats
                .Select(chat => new ChatResponse
                {
                    Id = chat.Id,
                    CustomerId = chat.CustomerId,
                    CustomerName = chat.Customer != null
                        ? $"{chat.Customer.FirstName} {chat.Customer.LastName}"
                        : "Customer",
                    AdminId = chat.AdminId,
                    CreatedAt = chat.CreatedAt,

                    UnreadMessages = chat.Messages.Count(m =>
                        m.SenderId != adminId &&
                        !m.IsRead),

                    LastMessageAt = chat.Messages
                        .Select(m => (DateTime?)m.SentAt)
                        .Max()
                })
                .ToList();

            return Ok(new ApiResponse<List<ChatResponse>>
            {
                IsSuccess = true,
                Message = "Chats retrieved successfully",
                Data = response
            });
        }


        // GET: api/Admin/Chats/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var adminId = _userManager.GetUserId(User);

            if (adminId == null)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "User is not authenticated."
                });
            }

            var chat = await _chatService.GetAdminChatAsync(
                id,
                adminId
            );

            if (chat == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Chat not found."
                });
            }

            var response = new ChatDetailsResponse
            {
                Id = chat.Id,

                CustomerId = chat.CustomerId,

                CustomerName = chat.Customer != null
                    ? $"{chat.Customer.FirstName} {chat.Customer.LastName}"
                    : "Customer",

                AdminId = chat.AdminId,

                CreatedAt = chat.CreatedAt,

                Messages = chat.Messages
                    .OrderBy(m => m.SentAt)
                    .Select(m => new ChatMessageResponse
                    {
                        Id = m.Id,
                        ChatId = m.ChatId,
                        OrderId = m.OrderId,
                        SenderId = m.SenderId,
                        Message = m.Message,
                        SentAt = m.SentAt,
                        IsRead = m.IsRead
                    })
                    .ToList()
            };

            await _chatService.MarkMessagesAsReadAsync(
                id,
                adminId
            );

            return Ok(new ApiResponse<ChatDetailsResponse>
            {
                IsSuccess = true,
                Message = "Chat retrieved successfully",
                Data = response
            });
        }
    }
}
