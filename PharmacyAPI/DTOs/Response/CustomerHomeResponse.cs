namespace PharmacyAPI.DTOs.Response
{
    public class CustomerHomeResponse
    {
        public List<CategoryItemResponse> Categories { get; set; }
            = new List<CategoryItemResponse>();

        public List<ProductResponse> Products { get; set; }
            = new List<ProductResponse>();

        public List<OrderResponse> RecentOrders { get; set; }
            = new List<OrderResponse>();
    }
}