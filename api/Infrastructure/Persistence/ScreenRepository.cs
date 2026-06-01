using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class ScreenRepository : IScreenRepository
{
    private readonly SichiDbContext _db;

    public ScreenRepository(SichiDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Screen>> GetApprovedAsync() =>
        await _db.Screens
            .Include(s => s.CurrentPlaylist)
            .Where(s => s.IsApproved)
            .ToListAsync();

    public async Task<IEnumerable<Screen>> GetPendingAsync() =>
        await _db.Screens
            .Where(s => !s.IsApproved)
            .ToListAsync();

    public async Task<Screen?> GetByIdAsync(int id) =>
        await _db.Screens
            .Include(s => s.CurrentPlaylist)
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<Screen?> GetByScreenKeyAsync(string screenKey) =>
        await _db.Screens
            .FirstOrDefaultAsync(s => s.ScreenKey == screenKey);

    public async Task AddAsync(Screen screen)
    {
        _db.Screens.Add(screen);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Screen screen)
    {
        _db.Screens.Update(screen);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var screen = await _db.Screens.FindAsync(id);
        if (screen is null)
            return;

        _db.Screens.Remove(screen);
        await _db.SaveChangesAsync();
    }
}
