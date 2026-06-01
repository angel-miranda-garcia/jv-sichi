using Domain.Exceptions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Playlists;

public class RemoveMediaFromPlaylistUseCase
{
    private readonly SichiDbContext _db;

    public RemoveMediaFromPlaylistUseCase(SichiDbContext db)
    {
        _db = db;
    }

    public async Task ExecuteAsync(int playlistId, int mediaItemId)
    {
        var playlistMedia = await _db.PlaylistMedias
            .FirstOrDefaultAsync(pm =>
                pm.PlaylistId == playlistId && pm.MediaItemId == mediaItemId)
            ?? throw new NotFoundException(
                $"Media item {mediaItemId} was not found in playlist {playlistId}.");

        _db.PlaylistMedias.Remove(playlistMedia);
        await _db.SaveChangesAsync();

        var playlist = await _db.Playlists.FindAsync(playlistId)
            ?? throw new NotFoundException($"Playlist with id {playlistId} was not found.");

        playlist.Version++;
        await _db.SaveChangesAsync();
    }
}
