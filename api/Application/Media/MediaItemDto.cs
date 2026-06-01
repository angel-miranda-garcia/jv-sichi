namespace Application.Media;

public record MediaItemDto(
    int Id,
    string FileName,
    string FilePath,
    string MediaType,
    int DurationSeconds,
    string Checksum,
    long FileSize,
    DateTime CreatedAt);
