namespace Application.Playlists;

public record PlaylistDto(
    int Id,
    string Name,
    int Version,
    bool IsActive,
    int ItemCount,
    string TotalDuration,
    DateTime CreatedAt);
