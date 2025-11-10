using System.ComponentModel.DataAnnotations;

namespace Reconova.Data.DTOs.User
{
    public class VerifyOtpDTO
    {
        [Required]
        public string OtpCode { get; set; }
    }
}
