using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Playlists;

public class UpdatePlaylistUseCase
{
    private readonly IPlaylistRepository _playlistRepository;
    private readonly IAuditService _auditService;

    public UpdatePlaylistUseCase(IPlaylistRepository playlistRepository, IAuditService auditService)
    {
        _playlistRepository = playlistRepository;
        _auditService = auditService;
    }

    public async Task<PlaylistDto> ExecuteAsync(int id, string name, int adminUserId)
    {
        var playlist = await _playlistRepository.GetByIdWithMediaAsync(id)
            ?? throw new NotFoundException($"Playlist with id {id} was not found.");

        playlist.Name = name;
        playlist.Version++;

        await _playlistRepository.UpdateAsync(playlist);

        await _auditService.LogAsync(
            "PlaylistUpdated",
            adminUserId,
            id,
            $"Playlist renamed to '{name}'");

        return PlaylistMapper.ToDto(playlist);
    }
}
