using Domain.Entities;

namespace Domain.Interfaces;

public interface IScreenRepository
{
    Task<IEnumerable<Screen>> GetApprovedAsync();
    Task<IEnumerable<Screen>> GetPendingAsync();
    Task<Screen?> GetByIdAsync(int id);
    Task<Screen?> GetByScreenKeyAsync(string screenKey);
    Task AddAsync(Screen screen);
    Task UpdateAsync(Screen screen);
    Task DeleteAsync(int id);
}
