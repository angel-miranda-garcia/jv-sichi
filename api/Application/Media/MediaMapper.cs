using Domain.Entities;
using Domain.Enums;

namespace Application.Media;

public static class MediaMapper
{
    public static MediaItemDto ToDto(MediaItem item) =>
        new(
            item.Id,
            item.FileName,
            item.FilePath,
            item.MediaType == MediaType.Video ? "Video" : item.MediaType.ToString(),
            item.DurationSeconds,
            item.Checksum,
            item.FileSize,
            item.CreatedAt);

    public static PlaylistUsageDto ToUsageDto(Playlist playlist) =>
        new(playlist.Id, playlist.Name);
}
