using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Playlists;

public class AssignPlaylistToScreenUseCase
{
    private readonly IScreenRepository _screenRepository;
    private readonly IPlaylistRepository _playlistRepository;
    private readonly IAuditService _auditService;
    private readonly IRealtimeService _realtimeService;

    public AssignPlaylistToScreenUseCase(
        IScreenRepository screenRepository,
        IPlaylistRepository playlistRepository,
        IAuditService auditService,
        IRealtimeService realtimeService)
    {
        _screenRepository = screenRepository;
        _playlistRepository = playlistRepository;
        _auditService = auditService;
        _realtimeService = realtimeService;
    }

    public async Task ExecuteAsync(int screenId, int? playlistId, int adminUserId)
    {
        var screen = await _screenRepository.GetByIdAsync(screenId)
            ?? throw new NotFoundException($"Screen with id {screenId} was not found.");

        Playlist? playlist = null;
        if (playlistId is not null)
        {
            playlist = await _playlistRepository.GetByIdWithMediaAsync(playlistId.Value)
                ?? throw new NotFoundException($"Playlist with id {playlistId} was not found.");
        }

        screen.CurrentPlaylistId = playlistId;
        await _screenRepository.UpdateAsync(screen);

        await _auditService.LogAsync(
            "PlaylistAssigned",
            adminUserId,
            screenId,
            $"Playlist {playlistId} assigned to screen {screenId}");

        if (playlistId is not null && playlist is not null)
        {
            await _realtimeService.SendPlaylistChangedAsync(
                screen.ScreenKey,
                playlistId.Value,
                playlist.Version);
        }
    }
}
