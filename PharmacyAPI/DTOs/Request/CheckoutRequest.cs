using PharmacyAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace PharmacyAPI.DTOs.Request
{
    public class CheckoutRequest
    {
        [Required]
        public PaymentMethod PaymentMethod { get; set; }
        [Required]
        [StringLength(300)]
        public string DeliveryAddress { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }
}