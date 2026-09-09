using PharmacyAPI.Models;

namespace PharmacyAPI.DTOs.Response
{
    public class CategoryResponse
    {
        public IEnumerable<Category> Categories { get; set; }
    }
}
