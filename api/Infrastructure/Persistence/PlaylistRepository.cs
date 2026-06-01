using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class PlaylistRepository : IPlaylistRepository
{
    private readonly SichiDbContext _db;

    public PlaylistRepository(SichiDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Playlist>> GetAllAsync() =>
        await _db.Playlists.ToListAsync();

    public async Task<Playlist?> GetByIdWithMediaAsync(int id) =>
        await _db.Playlists
            .Include(p => p.PlaylistMedias.OrderBy(pm => pm.SortOrder))
            .ThenInclude(pm => pm.MediaItem)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task AddAsync(Playlist playlist)
    {
        _db.Playlists.Add(playlist);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Playlist playlist)
    {
        _db.Playlists.Update(playlist);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var playlist = await _db.Playlists.FindAsync(id);
        if (playlist is null)
            return;

        _db.Playlists.Remove(playlist);
        await _db.SaveChangesAsync();
    }
}
