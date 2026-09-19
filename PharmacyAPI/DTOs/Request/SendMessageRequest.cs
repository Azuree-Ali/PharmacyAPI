using System.ComponentModel.DataAnnotations;

namespace PharmacyAPI.DTOs.Request
{
    public class SendMessageRequest
    {
        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;
    }
}
