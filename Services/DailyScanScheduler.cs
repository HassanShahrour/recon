using Microsoft.EntityFrameworkCore;
using Reconova.Core.Utilities;
using Reconova.Data;

namespace Reconova.Services
{
    public class DailyScanScheduler : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DailyScanScheduler> _logger;

        public DailyScanScheduler(IServiceProvider serviceProvider, ILogger<DailyScanScheduler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ReconovaDbContext>();
                var scanUtility = scope.ServiceProvider.GetRequiredService<ScanUtility>();

                try
                {
                    var now = DateTime.UtcNow.ToLocalTime();
                    var nowTime = TimeOnly.FromDateTime(now);
                    var minTime = nowTime.AddMinutes(-1);
                    var maxTime = nowTime.AddMinutes(1);

                    Console.WriteLine($"✅ Started!!!. {now},,,, {nowTime}");

                    var scansToCheck = await context.ScheduledScan
                        .Include(s => s.ToolsUsed)
                        .Where(s => s.IsActive &&
                                    s.Time >= minTime && s.Time <= maxTime)
                        .ToListAsync(stoppingToken);

                    var scansToRun = scansToCheck
                        .Where(s =>
                            Math.Abs((s.Time.ToTimeSpan() - nowTime.ToTimeSpan()).TotalMinutes) < 1)
                        .ToList();

                    Console.WriteLine($"✅ Found {scansToRun.Count} scans to run at {nowTime}.");

                    foreach (var scan in scansToRun)
                    {
                        if (scan.ToolsUsed == null || !scan.ToolsUsed.Any())
                            continue;

                        var tools = scan.ToolsUsed.Select(t => t.Tool).ToList();

                        foreach (var tool in tools)
                        {
                            await scanUtility.StartReconScanAsync(scan.UserId, scan.Target, tool, scan.TaskId)
                                .ContinueWith(task =>
                                {
                                    if (task.IsFaulted)
                                    {
                                        _logger.LogError(task.Exception, "❌ Tool: {Tool} Target: {Target}", tool, scan.Target);
                                        Console.WriteLine($"❌ Error running scan for tool {tool} on target {scan.Target}: {task.Exception?.Message}");
                                    }
                                    else
                                    {
                                        _logger.LogInformation("✅ Tool: {Tool} Target: {Target}", tool, scan.Target);
                                        Console.WriteLine($"✅ Successfully started scan for tool {tool} on target {scan.Target}.");
                                    }
                                }, stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while running scheduled scans.");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
