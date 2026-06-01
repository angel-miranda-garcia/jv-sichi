namespace Application.Playlists;

public record PlaylistDetailDto(
    int Id,
    string Name,
    int Version,
    bool IsActive,
    DateTime CreatedAt,
    IEnumerable<PlaylistMediaDto> Items);
