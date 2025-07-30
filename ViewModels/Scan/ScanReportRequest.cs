namespace Reconova.ViewModels.Scan
{
    public class ScanReportRequest
    {
        public string? UserId { get; set; }
        public int TaskId { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public bool IncludeDeleted { get; set; } = false;
    }
}
