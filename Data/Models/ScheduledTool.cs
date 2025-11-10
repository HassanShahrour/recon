using System.Text.Json.Serialization;

namespace Reconova.Data.Models
{
    public class ScheduledTool
    {
        public int Id { get; set; }
        public int ToolId { get; set; }
        public string Tool { get; set; } = null!;
        public int ScheduledScanId { get; set; }

        [JsonIgnore]
        public ScheduledScan ScheduledScan { get; set; } = null!;
    }
}
