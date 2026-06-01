using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Playlists;

public class AssignPlaylistToScreenUseCase
{
    private readonly IScreenRepository _screenRepository;
    private readonly IPlaylistRepository _playlistRepository;
    private readonly IAuditService _auditService;

    public AssignPlaylistToScreenUseCase(
        IScreenRepository screenRepository,
        IPlaylistRepository playlistRepository,
        IAuditService auditService)
    {
        _screenRepository = screenRepository;
        _playlistRepository = playlistRepository;
        _auditService = auditService;
    }

    public async Task ExecuteAsync(int screenId, int? playlistId, int adminUserId)
    {
        var screen = await _screenRepository.GetByIdAsync(screenId)
            ?? throw new NotFoundException($"Screen with id {screenId} was not found.");

        if (playlistId is not null)
        {
            _ = await _playlistRepository.GetByIdWithMediaAsync(playlistId.Value)
                ?? throw new NotFoundException($"Playlist with id {playlistId} was not found.");
        }

        screen.CurrentPlaylistId = playlistId;
        await _screenRepository.UpdateAsync(screen);

        await _auditService.LogAsync(
            "PlaylistAssigned",
            adminUserId,
            screenId,
            $"Playlist {playlistId} assigned to screen {screenId}");
    }
}
