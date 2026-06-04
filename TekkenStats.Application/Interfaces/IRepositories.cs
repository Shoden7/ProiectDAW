using TekkenStats.Domain.Entities;

namespace TekkenStats.Application.Interfaces;

public interface IPlayerRepository
{
    Task<Player?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Player?> GetByPolarisIdAsync(string polarisId, CancellationToken ct = default);
    Task<Player?> GetByTekkenUserIdAsync(long tekkenUserId, CancellationToken ct = default);
    Task<int> GetTotalCountAsync(CancellationToken ct);
    Task<IEnumerable<Player>> SearchByNameAsync(string name, int page, int pageSize, CancellationToken ct = default);
    Task<IEnumerable<Player>> GetLeaderboardAsync(int page, int pageSize, CancellationToken ct = default);
    Task<Player> UpsertAsync(Player player, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

public interface IMatchRepository
{
    Task<IEnumerable<Match>> GetMatchesByPlayerIdAsync(int playerId, int page, int pageSize, CancellationToken ct = default);
    Task<bool> ExistsByBattleIdAsync(string battleId, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<Match> matches, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

public interface IPlayerCharacterStatsRepository
{
    Task<IEnumerable<PlayerCharacterStats>> GetByPlayerIdAsync(int playerId, CancellationToken ct = default);
    Task UpsertAsync(PlayerCharacterStats stats, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

public interface IBookmarkRepository
{
    Task<IEnumerable<Bookmark>> GetByUserIdAsync(int userId, CancellationToken ct = default);
    Task<Bookmark?> GetAsync(int userId, int playerId, CancellationToken ct = default);
    Task AddAsync(Bookmark bookmark, CancellationToken ct = default);
    Task RemoveAsync(Bookmark bookmark, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
