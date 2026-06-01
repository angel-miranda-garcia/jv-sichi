using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class MediaRepository : IMediaRepository
{
    private readonly SichiDbContext _db;

    public MediaRepository(SichiDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<MediaItem>> GetAllAsync() =>
        await _db.MediaItems
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

    public async Task<MediaItem?> GetByIdAsync(int id) =>
        await _db.MediaItems.FindAsync(id);

    public async Task<IEnumerable<Playlist>> GetUsageAsync(int mediaItemId) =>
        await _db.PlaylistMedias
            .Where(pm => pm.MediaItemId == mediaItemId)
            .Select(pm => pm.Playlist)
            .Distinct()
            .ToListAsync();

    public async Task AddAsync(MediaItem item)
    {
        _db.MediaItems.Add(item);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _db.MediaItems.FindAsync(id);
        if (item is null)
            return;

        _db.MediaItems.Remove(item);
        await _db.SaveChangesAsync();
    }
}
