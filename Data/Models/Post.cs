using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Reconova.Data.Models
{
    public class Post
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string? Title { get; set; }

        [Required]
        public string? Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        public List<PostMedia> Media { get; set; } = new();

        [NotMapped]
        public List<IFormFile>? UploadedFiles { get; set; }

        public List<Comment> Comments { get; set; } = new();
        public List<Like> Likes { get; set; } = new();
    }
}
