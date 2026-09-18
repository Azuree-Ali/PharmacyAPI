using PharmacyAPI.Models;

namespace PharmacyAPI.DTOs.Response
{
    public class ProductFilterResponse
    {
        public IEnumerable<Product> Products { get; set; }
        public int TotalPages { get; set; }
        public int Page {  get; set; }
    }
}
