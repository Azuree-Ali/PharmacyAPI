using PharmacyAPI.Enums;

namespace PharmacyAPI.DTOs.Request
{
    public class CreateOrderRequest
    {
        public string OrderNumber { get; set; } = string.Empty;

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public decimal Discount { get; set; }

        public decimal DeliveryFees { get; set; }

        public PaymentMethod? PaymentMethod { get; set; }

        public string? DeliveryAddress { get; set; }

        public string? Notes { get; set; }

        public string ApplicationUserId { get; set; } = string.Empty;

        public List<OrderItemRequest> OrderItems { get; set; }
            = new List<OrderItemRequest>();
    }
}
