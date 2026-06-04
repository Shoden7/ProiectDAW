namespace TekkenStats.Domain.Entities;

public class Match
{
    public long Id { get; set; }

    // wavu.wiki battle_id — used to deduplicate on re-ingestion
    public string BattleId { get; set; } = string.Empty;

    public int Player1Id { get; set; }
    public int Player2Id { get; set; }

    public string Player1CharacterId { get; set; } = string.Empty;
    public string Player2CharacterId { get; set; } = string.Empty;

    public string Player1CharacterName { get; set; } = string.Empty;
    public string Player2CharacterName { get; set; } = string.Empty;

    // Rank at the time of the match (dan values from wavu)
    public int Player1RankDan { get; set; }
    public int Player2RankDan { get; set; }

    // 1 = Player1 won, 2 = Player2 won
    public int Winner { get; set; }

    public int Rounds { get; set; }

    public DateTime FoughtAt { get; set; }

    // Raw region string from wavu (e.g. "EU", "AS", "US")
    public string Region { get; set; } = string.Empty;

    public Player Player1 { get; set; } = null!;
    public Player Player2 { get; set; } = null!;
}
