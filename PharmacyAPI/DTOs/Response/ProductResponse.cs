using PharmacyAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyAPI.DTOs.Response
{
    public class ProductResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? GenericName { get; set; }
        public decimal Price { get; set; }
        public int MinStockLevel { get; set; }
        public bool RequiresPrescription { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
    }
}
