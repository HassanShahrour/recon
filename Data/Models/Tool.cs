using System.ComponentModel.DataAnnotations;

namespace Reconova.Data.Models
{
    public class Tool
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public string? Description { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public sbyte IsDeleted { get; set; } = 0;

        public Category? Category { get; set; }
    }
}
