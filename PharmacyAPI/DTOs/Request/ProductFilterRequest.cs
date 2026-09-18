using PharmacyAPI.Models;

namespace PharmacyAPI.DTOs.Request
{
    public class ProductFilterRequest
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Category { get; set; }
    }
}
