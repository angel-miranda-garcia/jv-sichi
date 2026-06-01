using Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Realtime;

public class PlayerHub : Hub
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PlayerHub> _logger;

    public PlayerHub(IServiceScopeFactory scopeFactory, ILogger<PlayerHub> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task ScreenConnected(string screenKey)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"screen_{screenKey}");
        _logger.LogInformation("Screen {ScreenKey} connected via SignalR", screenKey);
    }

    public async Task Heartbeat(string screenKey, DateTime _)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SichiDbContext>();

        var screen = await db.Screens
            .FirstOrDefaultAsync(s => s.ScreenKey == screenKey);

        if (screen is null)
        {
            _logger.LogWarning(
                "Heartbeat received for unknown screen key {ScreenKey}",
                screenKey);
            return;
        }

        screen.LastPing = DateTime.UtcNow;
        screen.IsOnline = true;
        await db.SaveChangesAsync();
    }
}
