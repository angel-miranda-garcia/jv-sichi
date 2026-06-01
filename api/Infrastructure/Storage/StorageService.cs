using System.Security.Cryptography;
using System.Text;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Storage;

public class StorageService : IStorageService
{
    private const string RelativePrefix = "uploads/media/";
    private readonly string _basePath;
    private readonly ILogger<StorageService> _logger;

    public StorageService(ILogger<StorageService> logger)
    {
        _logger = logger;
        _basePath = Environment.GetEnvironmentVariable("UPLOADS_PATH") ?? "/app/uploads/media";
    }

    public async Task<string> SaveFileAsync(Stream stream, string originalFilename)
    {
        var utcNow = DateTime.UtcNow;
        var subfolder = Path.Combine(utcNow.ToString("yyyy"), utcNow.ToString("MM"));
        var directory = Path.Combine(_basePath, subfolder);

        Directory.CreateDirectory(directory);

        var safeName = SanitizeFileName(originalFilename);
        var fileName = $"{safeName}_{Guid.NewGuid():N}.mp4";
        var absolutePath = Path.Combine(directory, fileName);

        await using var fileStream = new FileStream(
            absolutePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None);

        await stream.CopyToAsync(fileStream);

        return $"{RelativePrefix}{subfolder.Replace('\\', '/')}/{fileName}";
    }

    public Task DeleteFileAsync(string filePath)
    {
        var absolutePath = ResolveAbsolutePath(filePath);

        if (!File.Exists(absolutePath))
        {
            _logger.LogWarning("File not found for deletion: {FilePath}", filePath);
            return Task.CompletedTask;
        }

        File.Delete(absolutePath);
        return Task.CompletedTask;
    }

    public async Task<string> GetChecksumAsync(string filePath)
    {
        var absolutePath = ResolveAbsolutePath(filePath);

        await using var stream = new FileStream(
            absolutePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);

        var hash = await SHA256.HashDataAsync(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public string ResolveAbsolutePath(string relativeFilePath) =>
        Path.Combine(_basePath, GetRelativeSuffix(relativeFilePath));

    private static string GetRelativeSuffix(string relativeFilePath)
    {
        if (relativeFilePath.StartsWith(RelativePrefix, StringComparison.OrdinalIgnoreCase))
            return relativeFilePath[RelativePrefix.Length..].Replace('/', Path.DirectorySeparatorChar);

        return relativeFilePath.Replace('/', Path.DirectorySeparatorChar);
    }

    private static string SanitizeFileName(string originalFilename)
    {
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(originalFilename);
        var invalidChars = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(nameWithoutExtension.Length);

        foreach (var character in nameWithoutExtension)
            builder.Append(invalidChars.Contains(character) ? '_' : character);

        var sanitized = builder.ToString().Trim('_');
        return string.IsNullOrWhiteSpace(sanitized) ? "video" : sanitized;
    }
}
