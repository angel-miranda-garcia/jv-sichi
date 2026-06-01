namespace Application.Player;

public record PlayerPlaylistDto(
    int PlaylistId,
    int Version,
    IEnumerable<PlayerMediaItemDto> Items);
