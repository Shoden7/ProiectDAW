using Microsoft.EntityFrameworkCore;
using TekkenStats.Application.Interfaces;
using TekkenStats.Domain.Entities;
using TekkenStats.Infrastructure.Data;

namespace TekkenStats.Infrastructure.Repositories;

// ── 1. PLAYER REPOSITORY ──────────────────────────────────────────────────────
public class PlayerRepository(TekkenStatsDbContext context) : IPlayerRepository
{
    public async Task<Player?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await context.Players.FindAsync([id], ct);
    }

    public async Task<Player?> GetByTekkenUserIdAsync(long userId, CancellationToken ct = default)
    {
        return await context.Players
            .FirstOrDefaultAsync(p => p.TekkenUserId == userId, ct);
    }

    public async Task<Player?> GetByPolarisIdAsync(string polarisId, CancellationToken ct = default)
    {
        return await context.Players
            .FirstOrDefaultAsync(p => p.PolarisId == polarisId, ct);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken ct)
    {
        return await context.Players.CountAsync(ct);
    }

    public async Task<IEnumerable<Player>> SearchByNameAsync(
        string name, int page, int pageSize, CancellationToken ct = default)
    {
        return await context.Players
            .Where(p => p.PlayerName.ToLower().Contains(name.ToLower()))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Player>> GetLeaderboardAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        return await context.Players
            .AsNoTracking() 
            .OrderByDescending(p => p.DanRank)
            .ThenByDescending(p => p.Wins)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<Player> UpsertAsync(Player player, CancellationToken ct = default)
    {
        var existing = await context.Players
            .FirstOrDefaultAsync(p => p.TekkenUserId == player.TekkenUserId, ct);

        if (existing is null)
        {
            await context.Players.AddAsync(player, ct);
            return player;
        }

        existing.PlayerName = player.PlayerName ?? existing.PlayerName;
        existing.PolarisId = !string.IsNullOrEmpty(player.PolarisId) ? player.PolarisId : existing.PolarisId;
        existing.DanRank = player.DanRank;
        existing.CurrentRank = player.CurrentRank; 
        existing.MainCharacter = player.MainCharacter;
        existing.Wins = player.Wins;
        existing.Losses = player.Losses;
        existing.LastUpdated = DateTime.UtcNow;

        context.Players.Update(existing);
        return existing;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await context.SaveChangesAsync(ct);
    }
}

// ── 2. MATCH REPOSITORY ───────────────────────────────────────────────────────
public class MatchRepository(TekkenStatsDbContext context) : IMatchRepository
{
    public async Task<IEnumerable<Match>> GetMatchesByPlayerIdAsync(int playerId, int page, int pageSize, CancellationToken ct = default)
    {
        return await context.Matches
            .Include(m => m.Player1)
            .Include(m => m.Player2)
            .Where(m => m.Player1Id == playerId || m.Player2Id == playerId)
            .OrderByDescending(m => m.FoughtAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByBattleIdAsync(string battleId, CancellationToken ct = default)
    {
        return await context.Matches.AnyAsync(m => m.BattleId == battleId, ct);
    }

    public async Task AddRangeAsync(IEnumerable<Match> matches, CancellationToken ct = default)
    {
        await context.Matches.AddRangeAsync(matches, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await context.SaveChangesAsync(ct);
    }
}

// ── 3. PLAYER CHARACTER STATS REPOSITORY ──────────────────────────────────────
public class PlayerCharacterStatsRepository(TekkenStatsDbContext context) : IPlayerCharacterStatsRepository
{
    public async Task<IEnumerable<PlayerCharacterStats>> GetByPlayerIdAsync(int playerId, CancellationToken ct = default)
    {
        return await context.PlayerCharacterStats
            .Where(s => s.PlayerId == playerId)
            .ToListAsync(ct);
    }

    public async Task UpsertAsync(PlayerCharacterStats stats, CancellationToken ct = default)
    {
        var existing = await context.PlayerCharacterStats
            .FirstOrDefaultAsync(s => s.PlayerId == stats.PlayerId && s.CharacterId == stats.CharacterId, ct);

        if (existing is null)
        {
            await context.PlayerCharacterStats.AddAsync(stats, ct);
        }
        else
        {
            existing.Wins = stats.Wins;
            existing.Losses = stats.Losses;
            existing.Rank = stats.Rank;
            existing.DanRank = stats.DanRank;
            existing.UpdatedAt = DateTime.UtcNow;
            context.PlayerCharacterStats.Update(existing);
        }
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await context.SaveChangesAsync(ct);
    }
}

// ── 4. BOOKMARK REPOSITORY ────────────────────────────────────────────────────
public class BookmarkRepository(TekkenStatsDbContext context) : IBookmarkRepository
{
    public async Task<IEnumerable<Bookmark>> GetByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return await context.Bookmarks
            .Include(b => b.Player)
            .Where(b => b.UserId == userId)
            .ToListAsync(ct);
    }

    public async Task<Bookmark?> GetAsync(int userId, int playerId, CancellationToken ct = default)
    {
        return await context.Bookmarks
            .FirstOrDefaultAsync(b => b.UserId == userId && b.PlayerId == playerId, ct);
    }

    public async Task AddAsync(Bookmark bookmark, CancellationToken ct = default)
    {
        await context.Bookmarks.AddAsync(bookmark, ct);
    }

    public async Task RemoveAsync(Bookmark bookmark, CancellationToken ct = default)
    {
        context.Bookmarks.Remove(bookmark);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await context.SaveChangesAsync(ct);
    }
}

// ── 5. USER REPOSITORY ────────────────────────────────────────────────────────
public class UserRepository(TekkenStatsDbContext context) : IUserRepository
{
    public async Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await context.Users.FindAsync([id], ct);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Username == username, ct);
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await context.Users.AddAsync(user, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await context.SaveChangesAsync(ct);
    }
}

// ── 6. INGESTION STATE REPOSITORY ─────────────────────────────────────────────
public class IngestionStateRepository(TekkenStatsDbContext context) : IIngestionStateRepository
{
    // Matches: Task<long?> GetLastBattleIdAsync(CancellationToken ct = default);
    public async Task<long?> GetLastBattleAtAsync(CancellationToken ct = default)
    {
        var state = await context.IngestionStates.FirstOrDefaultAsync(ct);
        
        // Safely returns the nullable long unix timestamp column from your DB
        return state?.LastBattleId; 
    }

    // Matches: Task SetLastBattleIdAsync(long battleId, CancellationToken ct = default);
    public async Task SetLastBattleAtAsync(long battleId, CancellationToken ct = default)
    {
        var state = await context.IngestionStates.FirstOrDefaultAsync(ct);

        if (state is null)
        {
            state = new IngestionState 
            { 
                LastBattleId = battleId,
                UpdatedAt = DateTime.UtcNow 
            };
            await context.IngestionStates.AddAsync(state, ct);
        }
        else
        {
            state.LastBattleId = battleId;
            state.UpdatedAt = DateTime.UtcNow;
            context.IngestionStates.Update(state);
        }

        await context.SaveChangesAsync(ct);
    }
}