using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmacyAPI.DTOs.Response;
using PharmacyAPI.Models;
using PharmacyAPI.Services;
using PharmacyAPI.Utils;

namespace PharmacyAPI.Controllers
{
    [Authorize]
    [Area(CD.CUSTOMER_AREA)]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IOrderWorkflowService _orderWorkflowService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatController(
            IChatService chatService,
            IOrderWorkflowService orderWorkflowService,
            UserManager<ApplicationUser> userManager)
        {
            _chatService = chatService;
            _orderWorkflowService = orderWorkflowService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyChat()
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

            var chat = await _chatService.GetOrCreateCustomerChatAsync(
                userId
            );

            var response = new ChatDetailsResponse
            {
                Id = chat.Id,
                CustomerId = chat.CustomerId,
                AdminId = chat.AdminId,
                CreatedAt = chat.CreatedAt,

                Messages = chat.Messages
                    .OrderBy(m => m.SentAt)
                    .Select(m => new ChatMessageResponse
                    {
                        Id = m.Id,
                        ChatId = m.ChatId,
                        OrderId = m.OrderId,
                        Actions = GetActions(m.OffersDeliveryActions ? m.OrderId : null),
                        SenderId = m.SenderId,
                        Message = m.Message,
                        SentAt = m.SentAt,
                        IsRead = m.IsRead
                    })
                    .ToList()
            };

            await _chatService.MarkMessagesAsReadAsync(
                chat.Id,
                userId
            );

            return Ok(new ApiResponse<ChatDetailsResponse>
            {
                IsSuccess = true,
                Message = "Chat retrieved successfully",
                Data = response
            });
        }
        [HttpPost("{chatId}/messages")]
        public async Task<IActionResult> SendMessage(
            int chatId,
            SendMessageRequest request)
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

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Message cannot be empty."
                });
            }

            var chat = await _chatService.GetChatAsync(
                chatId,
                userId
            );

            if (chat == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = $"Chat {chatId} not found for user {userId}"
                });
            }

            var message = await _chatService.SendMessageAsync(
                chatId,
                userId,
                request.Message
            );

            if (message == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Unable to send message."
                });
            }

            var response = new ChatMessageResponse
            {
                Id = message.Id,
                ChatId = message.ChatId,
                OrderId = message.OrderId,
                Actions = GetActions(message.OffersDeliveryActions ? message.OrderId : null),
                SenderId = message.SenderId,
                Message = message.Message,
                SentAt = message.SentAt,
                IsRead = message.IsRead
            };

            await _orderWorkflowService.HandleCustomerChatReplyAsync(
                chatId,
                userId,
                request.Message);

            return Ok(new ApiResponse<ChatMessageResponse>
            {
                IsSuccess = true,
                Message = "Message sent successfully",
                Data = response
            });
        }

        private static List<ChatActionResponse> GetActions(int? orderId) => orderId.HasValue
            ?
            [
                new ChatActionResponse
                {
                    Label = "Arrived",
                    Href = $"/api/Customer/Orders/{orderId}/arrived"
                },
                new ChatActionResponse
                {
                    Label = "Cancel",
                    Href = $"/api/Customer/Orders/{orderId}/cancel"
                }
            ]
            : [];
    }
}
