using PharmacyAPI.Enums;

namespace PharmacyAPI.DTOs.Response
{
    public class OrderResponse
    {
        public int Id { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; }

        public OrderStatus Status { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal Discount { get; set; }

        public decimal DeliveryFees { get; set; }

        public decimal NetAmount { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public string? DeliveryAddress { get; set; }

        public string? Notes { get; set; }

        public string? ApplicationUserId { get; set; }
    }
}
