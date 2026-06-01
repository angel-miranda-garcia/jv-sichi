using Domain.Interfaces;

namespace Application.Playlists;

public class GetPlaylistsUseCase
{
    private readonly IPlaylistRepository _playlistRepository;

    public GetPlaylistsUseCase(IPlaylistRepository playlistRepository)
    {
        _playlistRepository = playlistRepository;
    }

    public async Task<IEnumerable<PlaylistDto>> ExecuteAsync()
    {
        var playlists = await _playlistRepository.GetAllAsync();
        var result = new List<PlaylistDto>();

        foreach (var playlist in playlists)
        {
            var withMedia = await _playlistRepository.GetByIdWithMediaAsync(playlist.Id);
            if (withMedia is not null)
                result.Add(PlaylistMapper.ToDto(withMedia));
        }

        return result;
    }
}
