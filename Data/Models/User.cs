using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Reconova.Data.Models
{
    public class User : IdentityUser
    {

        [Required]
        public string? FirstName { get; set; }

        [Required]
        public string? LastName { get; set; }

        [Required]
        public DateOnly BirthDate { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public sbyte IsDeleted { get; set; } = 0;

        //public string? Role { get; set; }

        //[NotMapped]
        //[Display(Name = "Choose a profile picture")]
        //public IFormFile? ProfilePhtoto { get; set; }

        //public string? ProfilePhotoPath { get; set; } = "~/images/account-bg.jpg";

        public ICollection<ScanResult>? ScanResults { get; set; }
        public ICollection<AIResult>? AIResults { get; set; }

    }
}
