namespace PharmacyAPI.DTOs.Request
{
    public class UpdateOrderItemRequest
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }
    }
}
