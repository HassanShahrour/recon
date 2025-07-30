using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Reconova.Data.Models
{
    [Table("ChatMessage")]
    public class ChatMessage
    {
        [Key]
        public string? Id { get; set; }

        public string? SenderId { get; set; }

        public string? RecipientId { get; set; }

        public string? Message { get; set; }

        public DateTimeOffset Time { get; set; }

        public sbyte IsDeleted { get; set; }

        public sbyte IsRead { get; set; }

        //[JsonIgnore]
        public User? Sender { get; set; }

        //[JsonIgnore]
        public User? Recipient { get; set; }
    }
}
