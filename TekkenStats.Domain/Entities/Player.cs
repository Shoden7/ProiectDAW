namespace TekkenStats.Domain.Entities;

public class Player
{
    public int Id { get; set; }
    public string PolarisId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public int TekkenUserId { get; set; }
    public string CurrentRank { get; set; } = string.Empty;
    public int DanRank { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public string MainCharacter { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public ICollection<Match> MatchesAsPlayer1 { get; set; } = new List<Match>();
    public ICollection<Match> MatchesAsPlayer2 { get; set; } = new List<Match>();
    public ICollection<PlayerCharacterStats> CharacterStats { get; set; } = new List<PlayerCharacterStats>();
    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
}
