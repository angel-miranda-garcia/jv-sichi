using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Playlists;

public class ReorderPlaylistUseCase
{
    private readonly IPlaylistRepository _playlistRepository;

    public ReorderPlaylistUseCase(IPlaylistRepository playlistRepository)
    {
        _playlistRepository = playlistRepository;
    }

    public async Task ExecuteAsync(int id, IEnumerable<ReorderItemDto> items)
    {
        var playlist = await _playlistRepository.GetByIdWithMediaAsync(id)
            ?? throw new NotFoundException($"Playlist with id {id} was not found.");

        foreach (var item in items)
        {
            var playlistMedia = playlist.PlaylistMedias
                .FirstOrDefault(pm => pm.MediaItemId == item.MediaItemId);

            if (playlistMedia is not null)
                playlistMedia.SortOrder = item.SortOrder;
        }

        playlist.Version++;
        await _playlistRepository.UpdateAsync(playlist);
    }
}
