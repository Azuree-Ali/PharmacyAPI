namespace PharmacyAPI.DTOs.Request
{
    public class CreateSalesInvoiceRequest
    {
        public string InvoiceNumber { get; set; } = string.Empty;

        public decimal Discount { get; set; }

        public int? CustomerId { get; set; }

        public int? OrderId { get; set; }

        public List<SalesInvoiceItemRequest> InvoiceItems { get; set; }
            = new List<SalesInvoiceItemRequest>();
    }
}
