namespace PharmacyAPI.Models
{
    public class OrderItemBatchAllocation
    {
        public int Id { get; set; }
        public int OrderItemId { get; set; }
        public OrderItem? OrderItem { get; set; }
        public int ProductBatchId { get; set; }
        public ProductBatch? ProductBatch { get; set; }
        public int Quantity { get; set; }
    }
}
