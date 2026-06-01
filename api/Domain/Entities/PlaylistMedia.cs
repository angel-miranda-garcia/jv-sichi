namespace Domain.Entities;

public class PlaylistMedia
{
    public int PlaylistId { get; set; }
    public Playlist Playlist { get; set; } = null!;
    public int MediaItemId { get; set; }
    public MediaItem MediaItem { get; set; } = null!;
    public int SortOrder { get; set; }
    public int? OverrideDuration { get; set; }
}
