namespace TekkenStats.Domain.Entities;

public class PlayerCharacterStats
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public int CharacterId { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public string Rank { get; set; } = string.Empty;
    public int DanRank { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Player Player { get; set; } = null!;
}
