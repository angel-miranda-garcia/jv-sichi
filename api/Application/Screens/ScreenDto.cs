namespace Application.Screens;

public record ScreenDto(
    int Id,
    string Name,
    string ScreenKey,
    string DeviceId,
    bool IsOnline,
    bool IsApproved,
    int? CurrentPlaylistId,
    string? CurrentPlaylistName,
    DateTime? LastPing,
    DateTime CreatedAt);
