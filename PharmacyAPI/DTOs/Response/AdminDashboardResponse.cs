namespace PharmacyAPI.DTOs.Response
{
    public class AdminDashboardResponse
    {
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalProductBatches { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalOrders { get; set; }
        public int TotalSalesInvoices { get; set; }

        public decimal TotalSales { get; set; }

        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
    }
}