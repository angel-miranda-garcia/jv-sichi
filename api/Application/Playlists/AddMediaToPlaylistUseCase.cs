using Domain.Entities;
using Domain.Exceptions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Playlists;

public class AddMediaToPlaylistUseCase
{
    private readonly SichiDbContext _db;

    public AddMediaToPlaylistUseCase(SichiDbContext db)
    {
        _db = db;
    }

    public async Task ExecuteAsync(
        int playlistId,
        int mediaItemId,
        int sortOrder,
        int? overrideDuration)
    {
        var playlist = await _db.Playlists.FindAsync(playlistId)
            ?? throw new NotFoundException($"Playlist with id {playlistId} was not found.");

        var mediaExists = await _db.MediaItems.AnyAsync(m => m.Id == mediaItemId);
        if (!mediaExists)
            throw new NotFoundException($"Media item with id {mediaItemId} was not found.");

        var alreadyExists = await _db.PlaylistMedias.AnyAsync(pm =>
            pm.PlaylistId == playlistId && pm.MediaItemId == mediaItemId);

        if (alreadyExists)
            throw new ConflictException("El ítem ya existe en la playlist");

        _db.PlaylistMedias.Add(new PlaylistMedia
        {
            PlaylistId = playlistId,
            MediaItemId = mediaItemId,
            SortOrder = sortOrder,
            OverrideDuration = overrideDuration
        });

        await _db.SaveChangesAsync();

        playlist.Version++;
        await _db.SaveChangesAsync();
    }
}
