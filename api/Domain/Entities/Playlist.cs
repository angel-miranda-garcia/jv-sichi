namespace Domain.Entities;

public class Playlist
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Version { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<PlaylistMedia> PlaylistMedias { get; set; } = new List<PlaylistMedia>();
}
