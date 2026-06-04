using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using TekkenStats.Application.Interfaces;
using TekkenStats.Domain.Entities;

namespace TekkenStats.Infrastructure.Services;

/// <summary>
/// Fetches replay batches from wavu.wiki/api/replays (requires Accept-Encoding: gzip)
/// and upserts players, character stats, and matches into the database.
/// </summary>
public class WavuReplayIngestionService(
    HttpClient httpClient,
    IPlayerRepository players,
    IMatchRepository matches,
    IPlayerCharacterStatsRepository charStats,
    IIngestionStateRepository state,
    ILogger<WavuReplayIngestionService> logger) : IWavuReplayIngestionService
{
    // Maps wavu dan values to readable rank names
    private static readonly Dictionary<int, string> DanToRank = new()
    {
        { 0, "Beginner" }, { 1, "1st Dan" }, { 2, "2nd Dan" }, { 3, "3rd Dan" },
        { 4, "Initiate" }, { 5, "Initiate" }, { 6, "Initiate" },
        { 7, "Mentor" }, { 8, "Mentor" }, { 9, "Mentor" },
        { 10, "Expert" }, { 11, "Expert" }, { 12, "Expert" },
        { 13, "Veteran" }, { 14, "Veteran" }, { 15, "Veteran" },
        { 16, "Warrior" }, { 17, "Warrior" }, { 18, "Warrior" },
        { 19, "Combatant" }, { 20, "Combatant" }, { 21, "Combatant" },
        { 22, "Brawler" }, { 23, "Brawler" }, { 24, "Brawler" },
        { 25, "Ranger" }, { 26, "Ranger" }, { 27, "Ranger" },
        { 28, "Cavalry" }, { 29, "Cavalry" }, { 30, "Cavalry" },
        { 31, "Warrior" }, { 32, "Fighter" }, { 33, "Strategist" },
        { 34, "Dominator" }, { 35, "Vanquisher" }, { 36, "Destroyer" },
        { 37, "Eliminator" }, { 38, "Garyu" }, { 39, "Shinryu" },
        { 40, "Tenryu" }, { 41, "Mighty Ruler" }, { 42, "Flame Ruler" },
        { 43, "Battle Ruler" }, { 44, "Fujin" }, { 45, "Raijin" },
        { 46, "Kishin" }, { 47, "Bushin" }, { 48, "Tekken King" },
        { 49, "Tekken Emperor" }, { 50, "Tekken God" },
        { 51, "Tekken God Supreme" }, { 52, "God of Destruction" }
    };

    // wavu character IDs -> names
    private static readonly Dictionary<int, string> CharacterNames = new()
    {
        { 0, "Paul" }, { 1, "Law" }, { 2, "King" }, { 3, "Yoshimitsu" },
        { 4, "Hwoarang" }, { 5, "Xiaoyu" }, { 6, "Jin" }, { 7, "Bryan" },
        { 8, "Kazuya" }, { 9, "Steve" }, { 10, "Jack-8" }, { 11, "Asuka" },
        { 12, "Devil Jin" }, { 13, "Feng" }, { 14, "Lili" }, { 15, "Dragunov" },
        { 16, "Leo" }, { 17, "Lars" }, { 18, "Alisa" }, { 19, "Claudio" },
        { 20, "Shaheen" }, { 21, "Nina" }, { 22, "Lee" }, { 23, "Kuma" },
        { 24, "Panda" }, { 28, "Zafina" }, { 29, "Leroy" }, { 32, "Jun" },
        { 33, "Reina" }, { 34, "Azucena" }, { 35, "Victor" }, { 36, "Raven" },
        { 37, "Eddy" }, { 38, "Lidia" }, { 39, "Heihachi" }, { 40, "Clive" }
    };

    public async Task<int> IngestBatchAsync(long? before = null, CancellationToken ct = default)
    {
        before ??= DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var url = $"https://wank.wavu.wiki/api/replays?before={before}";
        logger.LogInformation("Fetching replays before={Before} from {Url}", before, url);

        List<WavuReplay>? replays;
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept-Encoding", "gzip");
            var httpResponse = await httpClient.SendAsync(request, ct);
            httpResponse.EnsureSuccessStatusCode();

            await using var compressed = await httpResponse.Content.ReadAsStreamAsync(ct);
            await using var decompressed = new GZipStream(compressed, CompressionMode.Decompress);
            replays = await JsonSerializer.DeserializeAsync<List<WavuReplay>>(decompressed,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch replays from wavu.wiki");
            return 0;
        }

        if (replays is null || replays.Count == 0)
        {
            logger.LogInformation("No replays returned for before={Before}", before);
            return 0;
        }

        logger.LogInformation("Processing {Count} replays", replays.Count);

        int saved = 0;
        var newMatches = new List<Match>();

        // Collect all player data first so we can batch upsert
        var playerCache = new Dictionary<long, Player>(); // tekkenUserId -> entity

        foreach (var replay in replays)
        {
            if (await matches.ExistsByBattleIdAsync(replay.BattleId, ct)) continue;

            var p1 = await GetOrCreatePlayerAsync(replay, isPlayer1: true, playerCache, ct);
            var p2 = await GetOrCreatePlayerAsync(replay, isPlayer1: false, playerCache, ct);

            if (p1 is null || p2 is null) continue;

            await players.SaveChangesAsync(ct);

            // Re-fetch to get assigned IDs after save
            p1 = await players.GetByTekkenUserIdAsync(replay.P1UserId, ct) ?? p1;
            p2 = await players.GetByTekkenUserIdAsync(replay.P2UserId, ct) ?? p2;

            // Update character stats
            await UpdateCharacterStatsAsync(p1.Id, replay.P1CharaId, replay.P1Win == 1, ct);
            await UpdateCharacterStatsAsync(p2.Id, replay.P2CharaId, replay.P2Win == 1, ct);

            var match = new Match
            {
                BattleId = replay.BattleId,
                Player1Id = p1.Id,
                Player2Id = p2.Id,
                Player1CharacterId = replay.P1CharaId.ToString(),
                Player2CharacterId = replay.P2CharaId.ToString(),
                Player1CharacterName = CharacterNames.GetValueOrDefault(replay.P1CharaId, "Unknown"),
                Player2CharacterName = CharacterNames.GetValueOrDefault(replay.P2CharaId, "Unknown"),
                Player1RankDan = replay.P1Rank,
                Player2RankDan = replay.P2Rank,
                Winner = replay.P1Win == 1 ? 1 : 2,
                Rounds = replay.Rounds,
                FoughtAt = DateTimeOffset.FromUnixTimeSeconds(replay.BattleAt).UtcDateTime,
                Region = replay.Region ?? string.Empty
            };

            newMatches.Add(match);
            saved++;
        }

        if (newMatches.Count > 0)
        {
            await matches.AddRangeAsync(newMatches, ct);
            await matches.SaveChangesAsync(ct);
        }

        // Track the oldest timestamp in this batch as the cursor for the next run
        if (replays.Count > 0)
        {
            long minBattleAt = replays.Min(r => r.BattleAt);
            await state.SetLastBattleAtAsync(minBattleAt, ct);
        }

        logger.LogInformation("Ingested {Saved} new matches", saved);
        return saved;
    }

    private async Task<Player?> GetOrCreatePlayerAsync(
        WavuReplay replay, bool isPlayer1,
        Dictionary<long, Player> cache,
        CancellationToken ct)
    {
        long userId = isPlayer1 ? replay.P1UserId : replay.P2UserId;
        string name = isPlayer1 ? replay.P1Name : replay.P2Name;
        string polaris = isPlayer1 ? replay.P1PolarisId : replay.P2PolarisId;
        int dan = isPlayer1 ? replay.P1Rank : replay.P2Rank;
        int charaId = isPlayer1 ? replay.P1CharaId : replay.P2CharaId;
        bool won = isPlayer1 ? replay.P1Win == 1 : replay.P2Win == 1;

        if (cache.TryGetValue(userId, out var cached)) return cached;

        var player = new Player
        {
            TekkenUserId = userId,
            PlayerName = name ?? "Unknown",
            PolarisId = polaris ?? string.Empty,
            DanRank = dan,
            CurrentRank = DanToRank.GetValueOrDefault(dan, "Unknown"),
            MainCharacter = CharacterNames.GetValueOrDefault(charaId, "Unknown"),
            Wins = won ? 1 : 0,
            Losses = won ? 0 : 1,
            LastUpdated = DateTime.UtcNow
        };

        var result = await players.UpsertAsync(player, ct);
        cache[userId] = result;
        return result;
    }

    private async Task UpdateCharacterStatsAsync(
        int playerId, int charaId, bool won, CancellationToken ct)
    {
        var existing = (await charStats.GetByPlayerIdAsync(playerId, ct))
            .FirstOrDefault(s => s.CharacterId == charaId);

        if (existing is null)
        {
            await charStats.UpsertAsync(new PlayerCharacterStats
            {
                PlayerId = playerId,
                CharacterId = charaId,
                CharacterName = CharacterNames.GetValueOrDefault(charaId, "Unknown"),
                Wins = won ? 1 : 0,
                Losses = won ? 0 : 1,
                UpdatedAt = DateTime.UtcNow
            }, ct);
        }
        else
        {
            existing.Wins += won ? 1 : 0;
            existing.Losses += won ? 0 : 1;
            existing.UpdatedAt = DateTime.UtcNow;
            await charStats.UpsertAsync(existing, ct);
        }

        await charStats.SaveChangesAsync(ct);
    }
}

// ── wavu.wiki JSON shapes ─────────────────────────────────────────────────────


public class WavuReplay
{
    [JsonPropertyName("battle_id")]
    public string BattleId { get; set; } = string.Empty;

    [JsonPropertyName("battle_at")]
    public long BattleAt { get; set; }

    [JsonPropertyName("p1_user_id")]
    public long P1UserId { get; set; }

    [JsonPropertyName("p2_user_id")]
    public long P2UserId { get; set; }

    [JsonPropertyName("p1_name")]
    public string P1Name { get; set; } = string.Empty;

    [JsonPropertyName("p2_name")]
    public string P2Name { get; set; } = string.Empty;

    [JsonPropertyName("p1_polaris_id")]
    public string P1PolarisId { get; set; } = string.Empty;

    [JsonPropertyName("p2_polaris_id")]
    public string P2PolarisId { get; set; } = string.Empty;

    [JsonPropertyName("p1_chara_id")]
    public int P1CharaId { get; set; }

    [JsonPropertyName("p2_chara_id")]
    public int P2CharaId { get; set; }

    [JsonPropertyName("p1_rank")]
    public int P1Rank { get; set; }

    [JsonPropertyName("p2_rank")]
    public int P2Rank { get; set; }

    [JsonPropertyName("p1_win")]
    public int P1Win { get; set; }

    [JsonPropertyName("p2_win")]
    public int P2Win { get; set; }

    [JsonPropertyName("rounds")]
    public int Rounds { get; set; }

    [JsonPropertyName("region")]
    public string? Region { get; set; }
}
