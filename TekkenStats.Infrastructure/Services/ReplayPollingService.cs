using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TekkenStats.Application.Interfaces;

namespace TekkenStats.Infrastructure.Services;

/// <summary>
/// Runs as a .NET hosted background service.
/// Every PollInterval it calls wavu.wiki/api/replays and walks backward
/// through the timeline until it reaches already-seen data, then switches
/// to polling only the latest batch.
/// </summary>
public class ReplayPollingService(
    IServiceScopeFactory scopeFactory,
    ILogger<ReplayPollingService> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Replay polling service started. Interval: {Interval}", PollInterval);

        // On first start, do a catchup run walking back through history
        await CatchUpAsync(stoppingToken);

        using var timer = new PeriodicTimer(PollInterval);
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await PollLatestAsync(stoppingToken);
        }
    }

    /// <summary>
    /// Walk backward through replay batches (decrementing `before` by 700 seconds each time)
    /// until we hit a batch that produces zero new matches (all already seen).
    /// </summary>
    private async Task CatchUpAsync(CancellationToken ct)
    {
        logger.LogInformation("Starting catch-up ingestion from latest replays");

        using var scope = scopeFactory.CreateScope();
        var stateRepo = scope.ServiceProvider.GetRequiredService<IIngestionStateRepository>();
        var ingestion = scope.ServiceProvider.GetRequiredService<IWavuReplayIngestionService>();

        long? cursor = await stateRepo.GetLastBattleAtAsync(ct);

        if (cursor is null)
        {
            // First run: start from now, walk back ~30 days (approx 3600 batches of 700s)
            cursor = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            int maxBatches = 3600;

            for (int i = 0; i < maxBatches && !ct.IsCancellationRequested; i++)
            {
                int saved = await ingestion.IngestBatchAsync(cursor, ct);
                cursor -= 700;

                if (saved == 0 && i > 5) // allow a few empty batches before stopping
                {
                    logger.LogInformation("Catch-up complete after {Batches} batches", i + 1);
                    break;
                }

                // Brief pause to respect rate limits (one blocking request at a time per wavu docs)
                await Task.Delay(TimeSpan.FromMilliseconds(500), ct);
            }
        }
        else
        {
            logger.LogInformation("Resuming from saved cursor: {Cursor}", cursor);
        }
    }

    private async Task PollLatestAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var ingestion = scope.ServiceProvider.GetRequiredService<IWavuReplayIngestionService>();
            int saved = await ingestion.IngestBatchAsync(before: null, ct);
            if (saved > 0)
                logger.LogInformation("Poll: saved {Count} new matches", saved);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during replay poll");
        }
    }
}
