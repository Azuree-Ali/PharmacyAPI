namespace PharmacyAPI.DTOs.Response
{
    public class SalesInvoiceDetailsResponse
    {
        public int Id { get; set; }

        public string InvoiceNumber { get; set; } = string.Empty;

        public DateTime InvoiceDate { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal Discount { get; set; }

        public decimal NetAmount { get; set; }

        public int? CustomerId { get; set; }

        public int? OrderId { get; set; }

        public List<SalesInvoiceItemResponse> InvoiceItems { get; set; }
            = new List<SalesInvoiceItemResponse>();
    }
}
