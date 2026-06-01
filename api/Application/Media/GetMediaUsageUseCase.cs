using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Media;

public class GetMediaUsageUseCase
{
    private readonly IMediaRepository _mediaRepository;

    public GetMediaUsageUseCase(IMediaRepository mediaRepository)
    {
        _mediaRepository = mediaRepository;
    }

    public async Task<IEnumerable<PlaylistUsageDto>> ExecuteAsync(int mediaItemId)
    {
        var mediaItem = await _mediaRepository.GetByIdAsync(mediaItemId)
            ?? throw new NotFoundException($"Media item with id {mediaItemId} was not found.");

        var playlists = await _mediaRepository.GetUsageAsync(mediaItem.Id);
        return playlists.Select(MediaMapper.ToUsageDto);
    }
}
