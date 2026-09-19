namespace PharmacyAPI.DTOs.Response
{
    public class ChatResponse
    {
        public int Id { get; set; }

        public string CustomerId { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public string? AdminId { get; set; }

        public DateTime CreatedAt { get; set; }

        public int UnreadMessages { get; set; }

        public DateTime? LastMessageAt { get; set; }
    }
}
