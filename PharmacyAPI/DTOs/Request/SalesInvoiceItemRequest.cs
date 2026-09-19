namespace PharmacyAPI.DTOs.Request
{
    public class SalesInvoiceItemRequest
    {
        public int ProductBatchId { get; set; }

        public int Quantity { get; set; }
    }
}
