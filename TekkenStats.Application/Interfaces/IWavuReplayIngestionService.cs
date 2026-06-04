namespace TekkenStats.Application.Interfaces;

public interface IWavuReplayIngestionService
{
    /// <summary>
    /// Fetches replays from wavu.wiki/api/replays, decodes them,
    /// upserts players and character stats, and persists new matches.
    /// Returns the number of new matches saved.
    /// </summary>
    Task<int> IngestBatchAsync(long? before = null, CancellationToken ct = default);
}

public interface IIngestionStateRepository
{
    /// <summary>Gets the unix timestamp of the last successfully ingested batch boundary.</summary>
    Task<long?> GetLastBattleAtAsync(CancellationToken ct = default);
    Task SetLastBattleAtAsync(long battleAt, CancellationToken ct = default);
}
