namespace Application.Screens;

public record PendingScreenDto(
    int Id,
    string DeviceId,
    string ScreenKey,
    DateTime CreatedAt);
