namespace Reconova.Data.Models
{
    public class PostMedia
    {
        public int Id { get; set; }

        public string? PostId { get; set; }

        public string? FilePath { get; set; }

        public string? FileType { get; set; }

        public Post? Post { get; set; }
    }
}
