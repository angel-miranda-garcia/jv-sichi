using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Media;

public class DeleteMediaUseCase
{
    private readonly IMediaRepository _mediaRepository;
    private readonly IStorageService _storageService;
    private readonly IAuditService _auditService;

    public DeleteMediaUseCase(
        IMediaRepository mediaRepository,
        IStorageService storageService,
        IAuditService auditService)
    {
        _mediaRepository = mediaRepository;
        _storageService = storageService;
        _auditService = auditService;
    }

    public async Task ExecuteAsync(int id, int adminUserId)
    {
        var mediaItem = await _mediaRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Media item with id {id} was not found.");

        var fileName = mediaItem.FileName;

        await _storageService.DeleteFileAsync(mediaItem.FilePath);
        await _mediaRepository.DeleteAsync(id);

        await _auditService.LogAsync(
            "MediaDeleted",
            adminUserId,
            id,
            $"File '{fileName}' deleted");
    }
}
