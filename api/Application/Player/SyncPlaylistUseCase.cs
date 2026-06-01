using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Player;

public class SyncPlaylistUseCase
{
    private readonly IScreenRepository _screenRepository;
    private readonly IPlaylistRepository _playlistRepository;

    public SyncPlaylistUseCase(
        IScreenRepository screenRepository,
        IPlaylistRepository playlistRepository)
    {
        _screenRepository = screenRepository;
        _playlistRepository = playlistRepository;
    }

    public async Task<PlayerPlaylistDto> ExecuteAsync(string screenKey)
    {
        var screen = await _screenRepository.GetByScreenKeyAsync(screenKey)
            ?? throw new NotFoundException($"Screen with key '{screenKey}' was not found.");

        if (!screen.IsApproved)
            throw new ValidationException("Pantalla pendiente de aprobación");

        if (screen.CurrentPlaylistId is null)
            throw new NotFoundException("Sin playlist asignada");

        var playlist = await _playlistRepository.GetByIdWithMediaAsync(screen.CurrentPlaylistId.Value)
            ?? throw new NotFoundException($"Playlist with id {screen.CurrentPlaylistId} was not found.");

        var items = playlist.PlaylistMedias
            .OrderBy(pm => pm.SortOrder)
            .Select(pm => new PlayerMediaItemDto(
                pm.MediaItemId,
                pm.MediaItem.FilePath,
                pm.MediaItem.Checksum,
                pm.OverrideDuration ?? pm.MediaItem.DurationSeconds));

        return new PlayerPlaylistDto(playlist.Id, playlist.Version, items);
    }
}
