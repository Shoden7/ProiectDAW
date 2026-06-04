using Microsoft.EntityFrameworkCore;
using TekkenStats.Domain.Entities;

namespace TekkenStats.Infrastructure.Data;

public class TekkenStatsDbContext(DbContextOptions<TekkenStatsDbContext> options) : DbContext(options)
{
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<PlayerCharacterStats> PlayerCharacterStats => Set<PlayerCharacterStats>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Bookmark> Bookmarks => Set<Bookmark>();
    public DbSet<IngestionState> IngestionStates => Set<IngestionState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Player
        modelBuilder.Entity<Player>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasIndex(p => p.PolarisId).IsUnique();
            e.HasIndex(p => p.TekkenUserId).IsUnique();
            e.HasIndex(p => p.PlayerName);
            e.HasIndex(p => p.DanRank);
            e.Property(p => p.PolarisId).HasMaxLength(100);
            e.Property(p => p.PlayerName).HasMaxLength(100);
            e.Property(p => p.CurrentRank).HasMaxLength(50);
            e.Property(p => p.MainCharacter).HasMaxLength(50);
        });

        // Match
        modelBuilder.Entity<Match>(e =>
        {
            e.HasKey(m => m.Id);
            e.HasIndex(m => m.BattleId).IsUnique();
            e.HasIndex(m => m.FoughtAt);
            e.HasIndex(m => m.Player1Id);
            e.HasIndex(m => m.Player2Id);
            e.Property(m => m.BattleId).HasMaxLength(100);
            e.Property(m => m.Region).HasMaxLength(20);
            e.Property(m => m.Player1CharacterName).HasMaxLength(50);
            e.Property(m => m.Player2CharacterName).HasMaxLength(50);

            e.HasOne(m => m.Player1)
                .WithMany(p => p.MatchesAsPlayer1)
                .HasForeignKey(m => m.Player1Id)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(m => m.Player2)
                .WithMany(p => p.MatchesAsPlayer2)
                .HasForeignKey(m => m.Player2Id)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // PlayerCharacterStats
        modelBuilder.Entity<PlayerCharacterStats>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasIndex(s => new { s.PlayerId, s.CharacterId }).IsUnique();
            e.Property(s => s.CharacterName).HasMaxLength(50);
            e.Property(s => s.Rank).HasMaxLength(50);

            e.HasOne(s => s.Player)
                .WithMany(p => p.CharacterStats)
                .HasForeignKey(s => s.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // User
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.Username).IsUnique();
            e.Property(u => u.Username).HasMaxLength(50);
            e.Property(u => u.Email).HasMaxLength(200);
            e.Property(u => u.LinkedPolarisId).HasMaxLength(100);
        });

        // Bookmark
        modelBuilder.Entity<Bookmark>(e =>
        {
            e.HasKey(b => b.Id);
            e.HasIndex(b => new { b.UserId, b.PlayerId }).IsUnique();

            e.HasOne(b => b.User)
                .WithMany(u => u.Bookmarks)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(b => b.Player)
                .WithMany(p => p.Bookmarks)
                .HasForeignKey(b => b.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // IngestionState
        modelBuilder.Entity<IngestionState>(e =>
        {
            e.HasKey(s => s.Id);
        });
    }
}

// Single-row table to track ingestion progress
public class IngestionState
{
    public int Id { get; set; } = 1;
    public long LastBattleAt { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
