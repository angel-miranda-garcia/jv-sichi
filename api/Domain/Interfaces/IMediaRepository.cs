using Domain.Entities;

namespace Domain.Interfaces;

public interface IMediaRepository
{
    Task<IEnumerable<MediaItem>> GetAllAsync();
    Task<MediaItem?> GetByIdAsync(int id);
    Task<IEnumerable<Playlist>> GetUsageAsync(int mediaItemId);
    Task AddAsync(MediaItem item);
    Task DeleteAsync(int id);
}
