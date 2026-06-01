using System.Text;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Infrastructure.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Application.Media;

public class UploadMediaUseCase
{
    private const long MaxFileSizeBytes = 52_428_800;

    private readonly IStorageService _storageService;
    private readonly IMediaRepository _mediaRepository;
    private readonly IAuditService _auditService;
    private readonly ILogger<UploadMediaUseCase> _logger;

    public UploadMediaUseCase(
        IStorageService storageService,
        IMediaRepository mediaRepository,
        IAuditService auditService,
        ILogger<UploadMediaUseCase> logger)
    {
        _storageService = storageService;
        _mediaRepository = mediaRepository;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<MediaItemDto> ExecuteAsync(IFormFile file, int adminUserId)
    {
        if (file.Length > MaxFileSizeBytes)
            throw new ValidationException("El archivo supera el tamaño máximo de 50MB");

        await using var stream = file.OpenReadStream();

        if (!await IsValidMp4Async(stream))
            throw new ValidationException("Solo se permiten archivos MP4 H264");

        if (!string.Equals(Path.GetExtension(file.FileName), ".mp4", StringComparison.OrdinalIgnoreCase))
            throw new ValidationException("Solo se permiten archivos MP4 H264");

        stream.Position = 0;
        var filePath = await _storageService.SaveFileAsync(stream, file.FileName);
        var checksum = await _storageService.GetChecksumAsync(filePath);

        var absolutePath = ResolveAbsolutePath(filePath);
        var durationSeconds = MediaDurationHelper.GetDurationSeconds(absolutePath, _logger);

        var mediaItem = new MediaItem
        {
            FileName = file.FileName,
            FilePath = filePath,
            MediaType = MediaType.Video,
            DurationSeconds = durationSeconds,
            Checksum = checksum,
            FileSize = file.Length,
            CreatedAt = DateTime.UtcNow
        };

        await _mediaRepository.AddAsync(mediaItem);

        await _auditService.LogAsync(
            "MediaUploaded",
            adminUserId,
            mediaItem.Id,
            $"File '{file.FileName}' uploaded ({file.Length} bytes)");

        return MediaMapper.ToDto(mediaItem);
    }

    private static async Task<bool> IsValidMp4Async(Stream stream)
    {
        var header = new byte[12];
        var read = await stream.ReadAsync(header.AsMemory(0, 12));

        if (read < 8)
            return false;

        var ftyp = Encoding.ASCII.GetString(header, 4, 4);
        return ftyp == "ftyp";
    }

    private static string ResolveAbsolutePath(string relativeFilePath)
    {
        const string relativePrefix = "uploads/media/";
        var basePath = Environment.GetEnvironmentVariable("UPLOADS_PATH") ?? "/app/uploads/media";

        var suffix = relativeFilePath.StartsWith(relativePrefix, StringComparison.OrdinalIgnoreCase)
            ? relativeFilePath[relativePrefix.Length..]
            : relativeFilePath;

        return Path.Combine(basePath, suffix.Replace('/', Path.DirectorySeparatorChar));
    }
}
