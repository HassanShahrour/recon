namespace Reconova.BusinessLogic.DatabaseHelper.Interfaces
{
    public interface IDashboardRepository
    {
        public Task<Dictionary<string, int>> GetTopUsedToolsAsync(int count = 5);

        public Task<Dictionary<string, int>> GetUserRegistrationTrendAsync();

        public Task<Dictionary<string, int>> GetToolCategoryDistributionAsync();

        public Task<Dictionary<string, int>> GetTopFollowedUsersAsync();

        public Task<Dictionary<string, int>> GetMostActiveUsersAsync();
        public Task<Dictionary<string, int>> GetUserPlanDistributionAsync();
    }
}
