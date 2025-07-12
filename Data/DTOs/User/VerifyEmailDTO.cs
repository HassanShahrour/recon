using System.ComponentModel.DataAnnotations;

namespace Reconova.Data.DTOs.User
{
    public class VerifyEmailDTO
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; }
    }
}
