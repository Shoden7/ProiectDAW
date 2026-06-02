namespace TekkenStats.Application.DTOs.Player;

public class PlayerDto
{
    public int Id { get; set; }
    public string PolarisId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public string CurrentRank { get; set; } = string.Empty;
    public int DanRank { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public string MainCharacter { get; set; } = string.Empty;
    public double WinRate => Wins + Losses > 0 ? Math.Round((double)Wins / (Wins + Losses) * 100, 1) : 0;
    public int TotalMatches => Wins + Losses;
    public DateTime LastUpdated { get; set; }
    public List<CharacterStatDto> CharacterStats { get; set; } = new();
    public bool IsBookmarked { get; set; }
}

public class CharacterStatDto
{
    public string CharacterName { get; set; } = string.Empty;
    public int CharacterId { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public string Rank { get; set; } = string.Empty;
    public int DanRank { get; set; }
    public double WinRate => Wins + Losses > 0 ? Math.Round((double)Wins / (Wins + Losses) * 100, 1) : 0;
}

public class PlayerSearchResultDto
{
    public string PolarisId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public string CurrentRank { get; set; } = string.Empty;
    public int DanRank { get; set; }
    public string MainCharacter { get; set; } = string.Empty;
}
