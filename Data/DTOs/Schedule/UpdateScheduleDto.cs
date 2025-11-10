namespace Reconova.Data.DTOs.Schedule
{
    public class UpdateScheduleDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string Time { get; set; } = string.Empty;
        public List<string> ToolNames { get; set; } = new();
    }
}
