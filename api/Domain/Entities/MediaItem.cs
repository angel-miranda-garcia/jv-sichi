using Domain.Enums;

namespace Domain.Entities;

public class MediaItem
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public MediaType MediaType { get; set; }
    public int DurationSeconds { get; set; }
    public string Checksum { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<PlaylistMedia> PlaylistMedias { get; set; } = new List<PlaylistMedia>();
}
