using Domain.Entities;

namespace Application.Playlists;

public static class PlaylistMapper
{
    public static PlaylistDto ToDto(Playlist playlist, IEnumerable<PlaylistMedia>? medias = null)
    {
        var items = (medias ?? playlist.PlaylistMedias).ToList();
        var totalSeconds = items.Sum(GetItemDurationSeconds);

        return new PlaylistDto(
            playlist.Id,
            playlist.Name,
            playlist.Version,
            playlist.IsActive,
            items.Count,
            FormatDuration(totalSeconds),
            playlist.CreatedAt);
    }

    public static PlaylistDetailDto ToDetailDto(Playlist playlist)
    {
        var items = playlist.PlaylistMedias
            .OrderBy(pm => pm.SortOrder)
            .Select(ToMediaDto);

        return new PlaylistDetailDto(
            playlist.Id,
            playlist.Name,
            playlist.Version,
            playlist.IsActive,
            playlist.CreatedAt,
            items);
    }

    public static PlaylistMediaDto ToMediaDto(PlaylistMedia playlistMedia)
    {
        var duration = playlistMedia.OverrideDuration ?? playlistMedia.MediaItem.DurationSeconds;

        return new PlaylistMediaDto(
            playlistMedia.MediaItemId,
            playlistMedia.MediaItem.FileName,
            playlistMedia.MediaItem.FilePath,
            playlistMedia.SortOrder,
            duration,
            playlistMedia.OverrideDuration);
    }

    private static int GetItemDurationSeconds(PlaylistMedia playlistMedia) =>
        playlistMedia.OverrideDuration ?? playlistMedia.MediaItem?.DurationSeconds ?? 0;

    private static string FormatDuration(int totalSeconds)
    {
        if (totalSeconds <= 0)
            return "00:00:00";

        var hours = totalSeconds / 3600;
        var minutes = (totalSeconds % 3600) / 60;
        var seconds = totalSeconds % 60;
        return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
    }
}
