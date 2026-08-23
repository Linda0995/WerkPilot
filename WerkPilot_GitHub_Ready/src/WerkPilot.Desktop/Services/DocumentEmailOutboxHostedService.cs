using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WerkPilot.Application.Messaging;

namespace WerkPilot.Desktop.Services;

public sealed class DocumentEmailOutboxHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<DocumentEmailOutboxHostedService> logger)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ProcessOnceAsync(stoppingToken);

        using var timer = new PeriodicTimer(Interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
            await ProcessOnceAsync(stoppingToken);
    }

    private async Task ProcessOnceAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var service = scope.ServiceProvider
                .GetRequiredService<DocumentEmailService>();

            var result = await service.ProcessDueRetriesAsync(
                maximumCount: 20,
                cancellationToken);

            if (result.DueCount > 0)
            {
                logger.LogInformation(
                    "Belegversand-Warteschlange verarbeitet: {Due} fällig, {Sent} erfolgreich, {Failed} fehlgeschlagen.",
                    result.DueCount,
                    result.SentCount,
                    result.FailedCount);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Reguläres Beenden der Anwendung.
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Die automatische Belegversand-Warteschlange konnte nicht verarbeitet werden.");
        }
    }
}
