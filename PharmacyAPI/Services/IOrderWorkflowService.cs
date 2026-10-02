using PharmacyAPI.DTOs.Request;
using PharmacyAPI.Models;

namespace PharmacyAPI.Services
{
    public interface IOrderWorkflowService
    {
        Task<Order> CheckoutAsync(string customerId, CheckoutRequest request);
        Task<Order> CreateAdminOrderAsync(CreateOrderRequest request);
        Task<List<Order>> GetCustomerOrderHistoryAsync(string customerId);
        Task<Order?> GetCustomerOrderDetailsAsync(string customerId, int orderId);
        Task<Order?> ConfirmArrivalAsync(string customerId, int orderId);
        Task<Order?> CancelAsync(string customerId, int orderId);
        Task HandleCustomerChatReplyAsync(int chatId, string customerId, string message);
        Task DispatchDueDeliveryPromptsAsync(CancellationToken cancellationToken);
    }
}
