using Domain.Entities;
using Domain.Interfaces;

namespace Application.Playlists;

public class CreatePlaylistUseCase
{
    private readonly IPlaylistRepository _playlistRepository;
    private readonly IAuditService _auditService;

    public CreatePlaylistUseCase(IPlaylistRepository playlistRepository, IAuditService auditService)
    {
        _playlistRepository = playlistRepository;
        _auditService = auditService;
    }

    public async Task<PlaylistDto> ExecuteAsync(string name, int adminUserId)
    {
        var playlist = new Playlist
        {
            Name = name,
            Version = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _playlistRepository.AddAsync(playlist);

        await _auditService.LogAsync(
            "PlaylistCreated",
            adminUserId,
            playlist.Id,
            $"Playlist '{name}' created");

        return PlaylistMapper.ToDto(playlist);
    }
}
