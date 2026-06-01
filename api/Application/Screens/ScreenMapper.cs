using Domain.Entities;

namespace Application.Screens;

public static class ScreenMapper
{
    public static ScreenDto ToDto(Screen screen) =>
        new(
            screen.Id,
            screen.Name,
            screen.ScreenKey,
            screen.DeviceId,
            screen.IsOnline,
            screen.IsApproved,
            screen.CurrentPlaylistId,
            screen.CurrentPlaylist?.Name,
            screen.LastPing,
            screen.CreatedAt);

    public static PendingScreenDto ToPendingDto(Screen screen) =>
        new(screen.Id, screen.DeviceId, screen.ScreenKey, screen.CreatedAt);
}
