using System.ComponentModel.DataAnnotations;

namespace PharmacyAPI.DTOs.Request
{
    public class CreateCustomerRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        public string? Phone { get; set; }

        public decimal CurrentBalance { get; set; }
    }
}
