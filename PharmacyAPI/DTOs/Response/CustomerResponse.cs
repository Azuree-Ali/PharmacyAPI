namespace PharmacyAPI.DTOs.Response
{
    public class CustomerResponse
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public decimal CurrentBalance { get; set; }
    }
}
