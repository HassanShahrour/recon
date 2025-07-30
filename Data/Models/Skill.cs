namespace Reconova.Data.Models
{
    public class Skill
    {
        public int Id { get; set; }

        public string? UserId { get; set; }
        public string? Name { get; set; }

        public User? User { get; set; }
    }
}
