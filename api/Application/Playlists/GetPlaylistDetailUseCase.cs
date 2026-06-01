using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Playlists;

public class GetPlaylistDetailUseCase
{
    private readonly IPlaylistRepository _playlistRepository;

    public GetPlaylistDetailUseCase(IPlaylistRepository playlistRepository)
    {
        _playlistRepository = playlistRepository;
    }

    public async Task<PlaylistDetailDto> ExecuteAsync(int id)
    {
        var playlist = await _playlistRepository.GetByIdWithMediaAsync(id)
            ?? throw new NotFoundException($"Playlist with id {id} was not found.");

        return PlaylistMapper.ToDetailDto(playlist);
    }
}
