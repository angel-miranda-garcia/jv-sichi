namespace Domain.Entities;

public class Screen
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ScreenKey { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public int? CurrentPlaylistId { get; set; }
    public Playlist? CurrentPlaylist { get; set; }
    public bool IsOnline { get; set; }
    public bool IsApproved { get; set; }
    public DateTime? LastPing { get; set; }
    public DateTime CreatedAt { get; set; }
}
