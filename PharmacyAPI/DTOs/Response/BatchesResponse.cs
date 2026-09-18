using System.ComponentModel.DataAnnotations;

namespace PharmacyAPI.DTOs.Response
{
    public class BatchesResponse
    {
        public int Id { get; set; }
        [Required]
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        [Required]
        [StringLength(50)]
        public string BatchNumber { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.Date)]
        public DateTime ExpiryDate { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public decimal CostPrice { get; set; }
        [Required]
        [Range(0, int.MaxValue)]
        public int QuantityOnHand { get; set; }
    }
}
