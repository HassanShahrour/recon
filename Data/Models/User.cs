using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        public string? Role { get; set; }

        [MaxLength(500)]
        public string? Bio { get; set; }

        public string? Header { get; set; }

        public string? Education { get; set; }


        [NotMapped]
        [Display(Name = "Choose a Profile Photo")]
        public IFormFile? ProfilePhoto { get; set; }

        public string? ProfilePhotoPath { get; set; } = "~/images/account-bg.jpg";


        [NotMapped]
        [Display(Name = "Choose a Cover Photo")]
        public IFormFile? CoverPhoto { get; set; }

        public string? CoverPhotoPath { get; set; } = "~/images/account-bg.jpg";


        public string? Country { get; set; }

        public DateTime? LastSeen { get; set; }

        public bool IsOnline { get; set; } = false;

        [ForeignKey("PlanId")]
        public int? PlanId { get; set; } = null;
        public DateTime PlanStartDate { get; set; }
        public DateTime PlanEndDate { get; set; }
        public bool IsPlanActive { get; set; }
        public bool CanGenerateReport { get; set; } = false;

        public ICollection<ScanResult>? ScanResults { get; set; }
        public ICollection<AIResult>? AIResults { get; set; }

        public ICollection<UserFollowing>? Following { get; set; }
        public ICollection<UserFollowing>? Followers { get; set; }

        public ICollection<ChatMessage>? SentMessages { get; set; }
        public ICollection<ChatMessage>? ReceivedMessages { get; set; }

        public ICollection<Skill>? Skills { get; set; }
        public ICollection<Post>? Posts { get; set; }

        public ICollection<Tasks>? Tasks { get; set; }
        public Plan? Plan { get; set; }
    }
}
