namespace Domain.Interfaces;

public interface IRealtimeService
{
    Task SendPlaylistChangedAsync(string screenKey, int playlistId, int version);
    Task SendForceRefreshAsync(string screenKey);
}
