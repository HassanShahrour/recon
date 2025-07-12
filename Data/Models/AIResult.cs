using System.ComponentModel.DataAnnotations;

namespace Reconova.Data.Models
{
    public class AIResult
    {

        [Key]
        public int Id { get; set; }

        [Required]
        public string ScanId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string? UserId { get; set; }

        public string? Output { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public sbyte IsDeleted { get; set; } = 0;

        public User? User { get; set; }

        public ScanResult? ScanResult { get; set; }
    }
}
