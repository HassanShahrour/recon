using System.ComponentModel.DataAnnotations;

namespace Reconova.Data.Models
{
    public class ScanResult
    {
        [Key]
        public int Id { get; set; }

        public string ScanId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string? UserId { get; set; }

        public int TaskId { get; set; }

        [Required]
        public string? Target { get; set; }

        public string? Tool { get; set; }

        public string? Command { get; set; }

        public string? Output { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public sbyte IsDeleted { get; set; } = 0;

        public User? User { get; set; }

        public AIResult? AIResult { get; set; }

        public Tasks? Task { get; set; }
    }
}
