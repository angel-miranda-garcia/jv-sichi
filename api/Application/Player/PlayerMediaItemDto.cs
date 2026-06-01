namespace Application.Player;

public record PlayerMediaItemDto(
    int MediaItemId,
    string FilePath,
    string Checksum,
    int Duration);
