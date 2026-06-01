using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class SichiDbContext : DbContext
{
    public SichiDbContext(DbContextOptions<SichiDbContext> options)
        : base(options)
    {
    }

    public DbSet<Screen> Screens => Set<Screen>();
    public DbSet<Playlist> Playlists => Set<Playlist>();
    public DbSet<MediaItem> MediaItems => Set<MediaItem>();
    public DbSet<PlaylistMedia> PlaylistMedias => Set<PlaylistMedia>();
    public DbSet<User> Users => Set<User>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Screen>(entity =>
        {
            entity.HasIndex(e => e.ScreenKey).IsUnique();

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.ScreenKey).HasMaxLength(64);
            entity.Property(e => e.DeviceId).HasMaxLength(20);
            entity.Property(e => e.IsOnline).HasDefaultValue(false);
            entity.Property(e => e.IsApproved).HasDefaultValue(false);

            entity.HasOne(e => e.CurrentPlaylist)
                .WithMany()
                .HasForeignKey(e => e.CurrentPlaylistId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Playlist>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Version).HasDefaultValue(1);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<MediaItem>(entity =>
        {
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.MediaType).HasConversion<string>();
        });

        modelBuilder.Entity<PlaylistMedia>(entity =>
        {
            entity.HasKey(e => new { e.PlaylistId, e.MediaItemId });

            entity.HasOne(e => e.Playlist)
                .WithMany(p => p.PlaylistMedias)
                .HasForeignKey(e => e.PlaylistId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.MediaItem)
                .WithMany(m => m.PlaylistMedias)
                .HasForeignKey(e => e.MediaItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();

            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Role).HasMaxLength(50).HasDefaultValue("Admin");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.Detail).HasColumnType("text");

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
