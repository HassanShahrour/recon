namespace Reconova.Data.Models
{
    public class Plan
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }
        public int MaxScansPerDay { get; set; } = 0;
        public bool CanGenerateReport { get; set; } = false;
        public int Priority { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public sbyte IsDeleted { get; set; } = 0;
        public List<Tool> Tools { get; set; } = new List<Tool>();
    }
}
