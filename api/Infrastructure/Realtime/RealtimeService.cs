using Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Realtime;

public class RealtimeService : IRealtimeService
{
    private readonly IHubContext<PlayerHub> _hubContext;

    public RealtimeService(IHubContext<PlayerHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task SendPlaylistChangedAsync(string screenKey, int playlistId, int version) =>
        _hubContext.Clients
            .Group($"screen_{screenKey}")
            .SendAsync("playlistChanged", new { playlistId, version });

    public Task SendForceRefreshAsync(string screenKey) =>
        _hubContext.Clients
            .Group($"screen_{screenKey}")
            .SendAsync("forceRefresh", new { });
}
