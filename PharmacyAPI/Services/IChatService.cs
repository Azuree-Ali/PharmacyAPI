using PharmacyAPI.Models;

namespace PharmacyAPI.Services
{
    public interface IChatService
    {
        Task<Chat?> GetChatAsync(
            int chatId,
            string userId);

        Task<Chat?> GetCustomerChatAsync(
            string customerId);

        Task<List<Chat>> GetCustomerChatsAsync(
            string customerId);

        Task<List<Chat>> GetAdminChatsAsync(
            string adminId);

        Task<Chat?> GetAdminChatAsync(
            int chatId,
            string adminId);

        Task<Chat> GetOrCreateCustomerChatAsync(
            string customerId);

        Task<Chat> CreateChatAsync(
            string customerId,
            string? adminId = null);

        Task<List<ChatMessage>> GetMessagesAsync(
            int chatId);

        Task<ChatMessage?> SendMessageAsync(
            int chatId,
            string senderId,
            string message);

        Task MarkMessagesAsReadAsync(
            int chatId,
            string userId);
        Task<Chat?> GetChatForUserAsync(
    int chatId,
    string userId,
    bool isAdmin);
    }
}
