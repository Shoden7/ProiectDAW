using TekkenStats.Application.DTOs.Player;
using TekkenStats.Application.Interfaces;

namespace TekkenStats.Application.Services;

public class PlayerService(
    IPlayerRepository players,
    IMatchRepository matches,
    IPlayerCharacterStatsRepository charStats)
{
    public async Task<PlayerDetailDto?> GetPlayerByPolarisIdAsync(string polarisId, CancellationToken ct = default)
    {
        var player = await players.GetByPolarisIdAsync(polarisId, ct);
        if (player is null) return null;
        var stats = await charStats.GetByPlayerIdAsync(player.Id, ct);
        return MapToDetail(player, stats);
    }

    public async Task<PlayerDetailDto?> GetPlayerByIdAsync(int id, CancellationToken ct = default)
    {
        var player = await players.GetByIdAsync(id, ct);
        if (player is null) return null;
        var stats = await charStats.GetByPlayerIdAsync(player.Id, ct);
        return MapToDetail(player, stats);
    }

    public async Task<PagedResult<PlayerSearchResultDto>> SearchPlayersAsync(
        string name, int page, int pageSize, CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);
        page = Math.Max(1, page);
        var results = await players.SearchByNameAsync(name, page, pageSize, ct);
        var dtos = results.Select(p => new PlayerSearchResultDto(
            p.Id, p.PolarisId, p.PlayerName, p.CurrentRank, p.DanRank, p.MainCharacter));
        // NOTE: total count would need a separate count query; returning page worth for now
        return new PagedResult<PlayerSearchResultDto>(dtos, page, pageSize, dtos.Count());
    }

    public async Task<PagedResult<LeaderboardEntryDto>> GetLeaderboardAsync(
    int page, int pageSize, CancellationToken ct = default)
{
    pageSize = Math.Clamp(pageSize, 1, 100);
    page = Math.Max(1, page);
    
    var results = await players.GetLeaderboardAsync(page, pageSize, ct);
    
    // ⚡ FIX: Pull the real overall count from your data repository table layout layer
    // If your IPlayerRepository doesn't expose a dynamic count method, use an aggregate counter line:
    int totalSystemPlayers = await players.GetTotalCountAsync(ct); 

    var dtos = results.Select((p, i) => new LeaderboardEntryDto(
        (page - 1) * pageSize + i + 1,
        p.Id, p.PolarisId, p.PlayerName, p.CurrentRank, p.DanRank,
        p.Wins, p.Losses, WinRate(p.Wins, p.Losses), p.MainCharacter)).ToList();

    return new PagedResult<LeaderboardEntryDto>(dtos, page, pageSize, totalSystemPlayers);
}

    public async Task<PagedResult<MatchDto>> GetPlayerMatchesAsync(
        int playerId, int page, int pageSize, CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);
        page = Math.Max(1, page);

        var player = await players.GetByIdAsync(playerId, ct);
        if (player is null) return new PagedResult<MatchDto>([], page, pageSize, 0);

        var matchList = await matches.GetMatchesByPlayerIdAsync(playerId, page, pageSize, ct);

        var dtos = matchList.Select(m =>
        {
            bool isP1 = m.Player1Id == playerId;
            var opponentId = isP1 ? m.Player2Id : m.Player1Id;
            var opponentName = isP1 ? m.Player2.PlayerName : m.Player1.PlayerName;
            var myChar = isP1 ? m.Player1CharacterName : m.Player2CharacterName;
            var oppChar = isP1 ? m.Player2CharacterName : m.Player1CharacterName;
            var myDan = isP1 ? m.Player1RankDan : m.Player2RankDan;
            var oppDan = isP1 ? m.Player2RankDan : m.Player1RankDan;
            var won = (isP1 && m.Winner == 1) || (!isP1 && m.Winner == 2);

            return new MatchDto(m.Id, m.BattleId, opponentId, opponentName,
                myChar, oppChar, myDan, oppDan, won, m.Rounds, m.FoughtAt, m.Region);
        });

        return new PagedResult<MatchDto>(dtos, page, pageSize, dtos.Count());
    }

   private static PlayerDetailDto MapToDetail(
    Domain.Entities.Player p,
    IEnumerable<Domain.Entities.PlayerCharacterStats> stats)
{
    var charDtos = stats.Select(s => new CharacterStatsDto(
        s.CharacterName, 
        s.CharacterId, 
        s.Wins, 
        s.Losses, 
        GetDynamicRankName(s.DanRank, p.CurrentRank), // ⚡ FIX: Use an accurate fallback helper
        s.DanRank,
        WinRate(s.Wins, s.Losses)));

    return new PlayerDetailDto(
        p.Id, p.PolarisId, p.PlayerName, p.CurrentRank, p.DanRank,
        p.Wins, p.Losses, p.MainCharacter, WinRate(p.Wins, p.Losses),
        p.LastUpdated, charDtos);
}

// Simple fallback string parser helper
private static string GetDynamicRankName(int danRank, string fallbackPlayerRank)
{
    return danRank switch
    {
        30 => "God of Destruction 1",
        31 => "God of Destruction 2",
        32 => "God of Destruction 3",
        33 => "God of Destruction 4",
        34 => "God of Destruction 5",
        35 => "God of Destruction 6",
        36 => "God of Destruction 7",
        37 => "God of Destruction Infinite",
        _ => fallbackPlayerRank 
    };
}

    private static double WinRate(int wins, int losses)
    {
        int total = wins + losses;
        return total == 0 ? 0 : Math.Round((double)wins / total * 100, 1);
    }
}
