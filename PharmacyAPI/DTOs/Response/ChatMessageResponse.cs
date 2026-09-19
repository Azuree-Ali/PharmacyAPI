namespace PharmacyAPI.DTOs.Response
{
    public class ChatMessageResponse
    {
        public int Id { get; set; }

        public int ChatId { get; set; }

        public string SenderId { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTime SentAt { get; set; }

        public bool IsRead { get; set; }
    }
}
