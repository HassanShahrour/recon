namespace Reconova.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        public Dictionary<string, int> ToolUsage { get; set; } = new();

        public Dictionary<string, int> RegistrationTrends { get; set; } = new();

        public Dictionary<string, int> CategoryDistribution { get; set; } = new();

        public Dictionary<string, int> TopFollowers { get; set; } = new();

        public Dictionary<string, int> ActivityScores { get; set; } = new();

        public Dictionary<string, int>? PlanDistribution { get; set; }


        public int TotalUsers { get; set; }
        public int TotalTools { get; set; }
        public int TotalCategories { get; set; }
        public int Total { get; set; }
    }
}
