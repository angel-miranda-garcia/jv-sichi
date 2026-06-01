using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Playlists;

public class DeletePlaylistUseCase
{
    private readonly IPlaylistRepository _playlistRepository;
    private readonly IAuditService _auditService;

    public DeletePlaylistUseCase(IPlaylistRepository playlistRepository, IAuditService auditService)
    {
        _playlistRepository = playlistRepository;
        _auditService = auditService;
    }

    public async Task ExecuteAsync(int id, int adminUserId)
    {
        var playlist = await _playlistRepository.GetByIdWithMediaAsync(id)
            ?? throw new NotFoundException($"Playlist with id {id} was not found.");

        var playlistName = playlist.Name;

        await _playlistRepository.DeleteAsync(id);

        await _auditService.LogAsync(
            "PlaylistDeleted",
            adminUserId,
            id,
            $"Playlist '{playlistName}' deleted");
    }
}
