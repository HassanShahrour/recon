using Microsoft.EntityFrameworkCore;
using Reconova.Data;

namespace Reconova.Services
{
    public class PlanExpiryCheckerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PlanExpiryCheckerService> _logger;

        public PlanExpiryCheckerService(IServiceProvider serviceProvider, ILogger<PlanExpiryCheckerService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Running plan expiry check...");

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ReconovaDbContext>();

                    var now = DateTime.UtcNow.ToLocalTime();

                    var usersWithExpiredPlans = await context.Users
                        .Where(u => (u.Plan.DurationInDays != -1) && u.IsPlanActive && u.PlanEndDate < now)
                        .ToListAsync(stoppingToken);

                    foreach (var user in usersWithExpiredPlans)
                    {
                        user.IsPlanActive = false;
                        user.CanGenerateReport = false;
                        user.PlanId = null;
                        user.PlanEndDate = now;
                        user.PlanStartDate = now;
                        _logger.LogInformation($"Plan expired for user: {user.UserName} (ID: {user.Id})");
                    }

                    if (usersWithExpiredPlans.Count > 0)
                    {
                        await context.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while checking plan expiries");
                }

                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
        }

    }
}
