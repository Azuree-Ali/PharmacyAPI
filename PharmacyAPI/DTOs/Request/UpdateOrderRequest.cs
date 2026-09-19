using PharmacyAPI.Enums;

namespace PharmacyAPI.DTOs.Request
{
    public class UpdateOrderRequest
    {
        public decimal Discount { get; set; }

        public decimal DeliveryFees { get; set; }

        public OrderStatus Status { get; set; }

        public PaymentMethod? PaymentMethod { get; set; }

        public string? DeliveryAddress { get; set; }

        public string ApplicationUserId { get; set; } = string.Empty;

        public List<UpdateOrderItemRequest>? OrderItems { get; set; }
    }
}
