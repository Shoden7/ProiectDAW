namespace TekkenStats.Domain.Entities;

public class Match
{
    public int Id { get; set; }
    public int Player1Id { get; set; }
    public int Player2Id { get; set; }
    public int WinnerId { get; set; }
    public string Player1Character { get; set; } = string.Empty;
    public string Player2Character { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
    public int Player1RoundWins { get; set; }
    public int Player2RoundWins { get; set; }
    public DateTime PlayedAt { get; set; }
    public string ExternalMatchId { get; set; } = string.Empty;

    public Player Player1 { get; set; } = null!;
    public Player Player2 { get; set; } = null!;
}
