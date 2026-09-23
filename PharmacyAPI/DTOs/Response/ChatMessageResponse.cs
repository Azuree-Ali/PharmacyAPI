namespace PharmacyAPI.DTOs.Response
{
    public class ChatMessageResponse
    {
        public int Id { get; set; }

        public int ChatId { get; set; }

        public int? OrderId { get; set; }

        public List<ChatActionResponse> Actions { get; set; } = new();

        public string SenderId { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTime SentAt { get; set; }

        public bool IsRead { get; set; }
    }

    public class ChatActionResponse
    {
        public string Label { get; set; } = string.Empty;
        public string Method { get; set; } = "POST";
        public string Href { get; set; } = string.Empty;
    }
}
