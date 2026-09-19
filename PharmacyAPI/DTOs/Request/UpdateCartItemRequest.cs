using System.ComponentModel.DataAnnotations;

namespace PharmacyAPI.DTOs.Request
{
    public class UpdateCartItemRequest
    {
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}