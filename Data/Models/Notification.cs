using System.ComponentModel.DataAnnotations;

namespace Reconova.Data.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public string? ReceiverId { get; set; }
        public string? SenderId { get; set; }
        public string? Message { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
        public sbyte IsRead { get; set; } = 0;
        public string? Type { get; set; }
        public User? Receiver { get; set; }
        public User? Sender { get; set; }
    }
}
