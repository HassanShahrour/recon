using System.ComponentModel.DataAnnotations;

namespace Reconova.Data.Models
{
    public class UserFollowing
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string? FollowerId { get; set; }
        public User? Follower { get; set; }

        [Required]
        public string? FolloweeId { get; set; }
        public User? Followee { get; set; }

        public DateTime FollowedAt { get; set; } = DateTime.UtcNow;
    }

}
