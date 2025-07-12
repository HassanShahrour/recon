using System.ComponentModel.DataAnnotations;

namespace Reconova.Data.Models
{
    public class Pc
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? IP { get; set; }

        [Required]
        public string? Description { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public sbyte IsDeleted { get; set; } = 0;
    }
}
