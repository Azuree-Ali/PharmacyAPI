namespace PharmacyAPI.DTOs.Response
{
    public class UserResponse
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public bool IsLocked { get; set; }
    }
}
