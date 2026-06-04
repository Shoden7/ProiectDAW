using Microsoft.EntityFrameworkCore;
using TekkenStats.Application.Interfaces;
using TekkenStats.Domain.Entities;
using TekkenStats.Infrastructure.Data;

namespace TekkenStats.Infrastructure.Repositories;

public class PlayerRepository(TekkenStatsDbContext db) : IPlayerRepository
{
    public Task<Player?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Players.Include(p => p.CharacterStats).FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Player?> GetByPolarisIdAsync(string polarisId, CancellationToken ct = default) =>
        db.Players.Include(p => p.CharacterStats).FirstOrDefaultAsync(p => p.PolarisId == polarisId, ct);

    public Task<Player?> GetByTekkenUserIdAsync(long tekkenUserId, CancellationToken ct = default) =>
        db.Players.FirstOrDefaultAsync(p => p.TekkenUserId == tekkenUserId, ct);

    public async Task<IEnumerable<Player>> SearchByNameAsync(string name, int page, int pageSize, CancellationToken ct = default) =>
        await db.Players
            .Where(p => EF.Functions.Like(p.PlayerName, $"%{name}%"))
            .OrderByDescending(p => p.DanRank)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task<IEnumerable<Player>> GetLeaderboardAsync(int page, int pageSize, CancellationToken ct = default) =>
        await db.Players
            .OrderByDescending(p => p.DanRank)
            .ThenByDescending(p => p.Wins)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task<Player> UpsertAsync(Player player, CancellationToken ct = default)
    {
        var existing = await db.Players.FirstOrDefaultAsync(
            p => p.TekkenUserId == player.TekkenUserId, ct);

        if (existing is null)
        {
            db.Players.Add(player);
            return player;
        }

        existing.PlayerName = player.PlayerName;
        existing.PolarisId = player.PolarisId;
        existing.CurrentRank = player.CurrentRank;
        existing.DanRank = player.DanRank;
        existing.Wins = player.Wins;
        existing.Losses = player.Losses;
        existing.MainCharacter = player.MainCharacter;
        existing.LastUpdated = DateTime.UtcNow;
        return existing;
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}

public class MatchRepository(TekkenStatsDbContext db) : IMatchRepository
{
    public async Task<IEnumerable<Match>> GetMatchesByPlayerIdAsync(
        int playerId, int page, int pageSize, CancellationToken ct = default) =>
        await db.Matches
            .Include(m => m.Player1)
            .Include(m => m.Player2)
            .Where(m => m.Player1Id == playerId || m.Player2Id == playerId)
            .OrderByDescending(m => m.FoughtAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public Task<bool> ExistsByBattleIdAsync(string battleId, CancellationToken ct = default) =>
        db.Matches.AnyAsync(m => m.BattleId == battleId, ct);

    public async Task AddRangeAsync(IEnumerable<Match> matches, CancellationToken ct = default) =>
        await db.Matches.AddRangeAsync(matches, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}

public class PlayerCharacterStatsRepository(TekkenStatsDbContext db) : IPlayerCharacterStatsRepository
{
    public async Task<IEnumerable<PlayerCharacterStats>> GetByPlayerIdAsync(int playerId, CancellationToken ct = default) =>
        await db.PlayerCharacterStats.Where(s => s.PlayerId == playerId).ToListAsync(ct);

    public async Task UpsertAsync(PlayerCharacterStats stats, CancellationToken ct = default)
    {
        var existing = await db.PlayerCharacterStats.FirstOrDefaultAsync(
            s => s.PlayerId == stats.PlayerId && s.CharacterId == stats.CharacterId, ct);

        if (existing is null)
        {
            await db.PlayerCharacterStats.AddAsync(stats, ct);
            return;
        }

        existing.Wins = stats.Wins;
        existing.Losses = stats.Losses;
        existing.Rank = stats.Rank;
        existing.DanRank = stats.DanRank;
        existing.UpdatedAt = DateTime.UtcNow;
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}

public class BookmarkRepository(TekkenStatsDbContext db) : IBookmarkRepository
{
    public async Task<IEnumerable<Bookmark>> GetByUserIdAsync(int userId, CancellationToken ct = default) =>
        await db.Bookmarks
            .Include(b => b.Player)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);

    public Task<Bookmark?> GetAsync(int userId, int playerId, CancellationToken ct = default) =>
        db.Bookmarks.FirstOrDefaultAsync(b => b.UserId == userId && b.PlayerId == playerId, ct);

    public async Task AddAsync(Bookmark bookmark, CancellationToken ct = default) =>
        await db.Bookmarks.AddAsync(bookmark, ct);

    public Task RemoveAsync(Bookmark bookmark, CancellationToken ct = default)
    {
        db.Bookmarks.Remove(bookmark);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}

public class UserRepository(TekkenStatsDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Username == username, ct);

    public async Task AddAsync(User user, CancellationToken ct = default) =>
        await db.Users.AddAsync(user, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}

public class IngestionStateRepository(TekkenStatsDbContext db) : IIngestionStateRepository
{
    public async Task<long?> GetLastBattleAtAsync(CancellationToken ct = default)
    {
        var state = await db.IngestionStates.FirstOrDefaultAsync(ct);
        return state?.LastBattleAt;
    }

    public async Task SetLastBattleAtAsync(long battleAt, CancellationToken ct = default)
    {
        var state = await db.IngestionStates.FirstOrDefaultAsync(ct);
        if (state is null)
        {
            db.IngestionStates.Add(new IngestionState { Id = 1, LastBattleAt = battleAt });
        }
        else
        {
            state.LastBattleAt = battleAt;
            state.UpdatedAt = DateTime.UtcNow;
        }
        await db.SaveChangesAsync(ct);
    }
}
