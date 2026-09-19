namespace PharmacyAPI.DTOs.Response
{
    public class ChatDetailsResponse
    {
        public int Id { get; set; }

        public string CustomerId { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public string? AdminId { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<ChatMessageResponse> Messages { get; set; }
            = new List<ChatMessageResponse>();
    }
}
