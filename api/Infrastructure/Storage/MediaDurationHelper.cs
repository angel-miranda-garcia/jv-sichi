using MediaInfo.DotNetWrapper;
using MediaInfo.DotNetWrapper.Enumerations;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Storage;

public static class MediaDurationHelper
{
    public static int GetDurationSeconds(string absolutePath, ILogger logger)
    {
        try
        {
            var mediaInfo = new MediaInfo.DotNetWrapper.MediaInfo();

            var openStatus = mediaInfo.Open(absolutePath);
            if (openStatus == Status.None)
            {
                logger.LogWarning("MediaInfo could not open file: {Path}", absolutePath);
                return 0;
            }

            var durationValue = mediaInfo.Get(StreamKind.Video, 0, "Duration");

            if (!int.TryParse(durationValue, out var durationMs) || durationMs <= 0)
            {
                logger.LogWarning(
                    "MediaInfo returned invalid duration for {Path}: {Duration}",
                    absolutePath,
                    durationValue);
                return 0;
            }

            return durationMs / 1000;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to extract duration for {Path}", absolutePath);
            return 0;
        }
    }
}
