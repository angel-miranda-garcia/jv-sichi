namespace Application.Playlists;

public record PlaylistMediaDto(
    int MediaItemId,
    string FileName,
    string FilePath,
    int SortOrder,
    int Duration,
    int? OverrideDuration);
