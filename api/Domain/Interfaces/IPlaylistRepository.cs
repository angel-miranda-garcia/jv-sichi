using Domain.Entities;

namespace Domain.Interfaces;

public interface IPlaylistRepository
{
    Task<IEnumerable<Playlist>> GetAllAsync();
    Task<Playlist?> GetByIdWithMediaAsync(int id);
    Task AddAsync(Playlist playlist);
    Task UpdateAsync(Playlist playlist);
    Task DeleteAsync(int id);
}
