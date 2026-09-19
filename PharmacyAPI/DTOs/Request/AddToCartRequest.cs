using System.ComponentModel.DataAnnotations;

namespace PharmacyAPI.DTOs.Request
{
    public class AddToCartRequest
    {
        [Required]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}