using System.ComponentModel.DataAnnotations;

namespace PharmacyAPI.DTOs.Request
{
    public class CreateUserRequest
    {
        [Required] 
        [StringLength(50)] public string FirstName { get; set; } = string.Empty;
        [Required]
        [StringLength(50)] public string LastName { get; set; } = string.Empty;
        [Required] 
        [EmailAddress] public string Email { get; set; } = string.Empty;
        [Phone]
        public string? PhoneNumber { get; set; } 
        public string? Address { get; set; }
        [Required] 
        [DataType(DataType.Password)] 
        public string Password { get; set; } = string.Empty;
        [Required]
        [Compare(nameof(Password))] 
        public string ConfirmPassword { get; set; } = string.Empty; 
        public List<string> SelectedRoles { get; set; }
            = new List<string>();
    }
}
