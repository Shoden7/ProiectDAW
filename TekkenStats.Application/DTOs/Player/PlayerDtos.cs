namespace TekkenStats.Application.DTOs.Player;

public record PlayerSummaryDto(
    int Id,
    string PolarisId,
    string PlayerName,
    string CurrentRank,
    int DanRank,
    int Wins,
    int Losses,
    string MainCharacter,
    double WinRate,
    DateTime LastUpdated
);

public record PlayerDetailDto(
    int Id,
    string PolarisId,
    string PlayerName,
    string CurrentRank,
    int DanRank,
    int Wins,
    int Losses,
    string MainCharacter,
    double WinRate,
    DateTime LastUpdated,
    IEnumerable<CharacterStatsDto> CharacterStats
);

public record CharacterStatsDto(
    string CharacterName,
    int CharacterId,
    int Wins,
    int Losses,
    string Rank,
    int DanRank,
    double WinRate
);

public record MatchDto(
    long Id,
    string BattleId,
    int OpponentId,
    string OpponentName,
    string MyCharacter,
    string OpponentCharacter,
    int MyRankDan,
    int OpponentRankDan,
    bool Won,
    int Rounds,
    DateTime FoughtAt,
    string Region
);

public record PlayerSearchResultDto(
    int Id,
    string PolarisId,
    string PlayerName,
    string CurrentRank,
    int DanRank,
    string MainCharacter
);

public record LeaderboardEntryDto(
    int Rank,
    int Id,
    string PolarisId,
    string PlayerName,
    string CurrentRank,
    int DanRank,
    int Wins,
    int Losses,
    double WinRate,
    string MainCharacter
);

public record PagedResult<T>(
    IEnumerable<T> Items,
    int Page,
    int PageSize,
    int TotalCount
);
