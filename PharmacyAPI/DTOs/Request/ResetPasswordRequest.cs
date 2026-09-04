using System.ComponentModel.DataAnnotations;

namespace Pharmacy.DTOs.Request
{
    public class ResetPasswordRequest
    {
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
        [DataType(DataType.Password), Compare(nameof(NewPassword))]
        public string ConfirmNewPassword { get; set; }
        public string UserId { get; set; }
        public string Token { get; set; }
    }
}
