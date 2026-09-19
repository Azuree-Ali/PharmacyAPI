namespace PharmacyAPI.DTOs.Response
{
    public class SalesInvoiceItemResponse
    {
        public int Id { get; set; }

        public int ProductBatchId { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }
    }
}
