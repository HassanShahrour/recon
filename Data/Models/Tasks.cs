using System.ComponentModel.DataAnnotations;

namespace Reconova.Data.Models
{
    public class Tasks
    {
        [Key]
        public int Id { get; set; }

        public string? UserId { get; set; }

        [Required]
        public string? Target { get; set; }

        public string? Description { get; set; }

        public int? Percentage { get; set; } = 0;

        public DateTime? LastModified { get; set; } = DateTime.UtcNow;

        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;

        public sbyte IsDeleted { get; set; } = 0;

        public User? User { get; set; }

        public ICollection<ScanResult>? ScanResults { get; set; }

        public ICollection<AIResult>? AIResults { get; set; }
    }
}
