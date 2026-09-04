namespace PharmacyAPI.DTOs.Request
{
    public class VerifyOTPRequest
    {
        public string OTP { get; set; }
        public string UserId { get; set; }
    }
}
