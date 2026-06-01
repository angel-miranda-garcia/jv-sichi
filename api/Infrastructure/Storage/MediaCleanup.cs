using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Storage;

public class MediaCleanup : BackgroundService
{
    private const string RelativePrefix = "uploads/media/";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MediaCleanup> _logger;
    private readonly string _uploadsPath;

    public MediaCleanup(IServiceScopeFactory scopeFactory, ILogger<MediaCleanup> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _uploadsPath = Environment.GetEnvironmentVariable("UPLOADS_PATH") ?? "/app/uploads/media";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunCleanupCycleAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MediaCleanup: error during cleanup cycle");
            }

            try
            {
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task RunCleanupCycleAsync(CancellationToken stoppingToken)
    {
        if (!Directory.Exists(_uploadsPath))
        {
            _logger.LogWarning(
                "MediaCleanup: uploads path does not exist: {Path}",
                _uploadsPath);
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SichiDbContext>();

        var registeredPaths = await db.MediaItems
            .Select(m => m.FilePath)
            .ToHashSetAsync(stoppingToken);

        var filesOnDisk = Directory.GetFiles(_uploadsPath, "*.mp4", SearchOption.AllDirectories);
        var normalizedBase = Path.GetFullPath(_uploadsPath)
            .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

        foreach (var absolutePath in filesOnDisk)
        {
            var relativePath = ToRelativeFilePath(absolutePath, normalizedBase);
            if (relativePath is null)
                continue;

            if (registeredPaths.Contains(relativePath))
                continue;

            File.Delete(absolutePath);
            _logger.LogInformation("MediaCleanup: deleted orphan file {Path}", relativePath);
        }
    }

    private static string? ToRelativeFilePath(string absolutePath, string normalizedBasePath)
    {
        var normalizedFile = Path.GetFullPath(absolutePath);

        if (!normalizedFile.StartsWith(normalizedBasePath, StringComparison.OrdinalIgnoreCase))
            return null;

        var suffix = normalizedFile[normalizedBasePath.Length..]
            .Replace(Path.DirectorySeparatorChar, '/')
            .TrimStart('/');

        return $"{RelativePrefix}{suffix}";
    }
}
