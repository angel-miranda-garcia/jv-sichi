using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

public class HeartbeatMonitor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<HeartbeatMonitor> _logger;

    public HeartbeatMonitor(
        IServiceScopeFactory scopeFactory,
        ILogger<HeartbeatMonitor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<SichiDbContext>();

                var cutoff = DateTime.UtcNow.AddSeconds(-60);
                var staleScreens = await db.Screens
                    .Where(s => s.IsOnline && s.LastPing < cutoff)
                    .ToListAsync(stoppingToken);

                foreach (var screen in staleScreens)
                    screen.IsOnline = false;

                if (staleScreens.Count > 0)
                {
                    await db.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation(
                        "HeartbeatMonitor: {Count} screens marked offline",
                        staleScreens.Count);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HeartbeatMonitor: error during heartbeat check");
            }
        }
    }
}
