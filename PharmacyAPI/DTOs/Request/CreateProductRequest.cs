using Microsoft.AspNetCore.Mvc.Rendering;

namespace PharmacyAPI.DTOs.Request
{
    public class CreateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? GenericName { get; set; }
        public decimal Price { get; set; }
        public int MinStockLevel { get; set; } = 5;
        public bool RequiresPrescription { get; set; } = false;
        public int CategoryId { get; set; }
    }
}
