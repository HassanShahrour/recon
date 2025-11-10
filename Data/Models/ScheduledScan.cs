namespace Reconova.Data.Models
{
    public class ScheduledScan
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string UserId { get; set; } = null!;
        public string Target { get; set; } = null!;
        public int TaskId { get; set; }
        public bool IsActive { get; set; } = true;
        public TimeOnly Time { get; set; }
        public List<ScheduledTool> ToolsUsed { get; set; } = new List<ScheduledTool>();
    }

}
