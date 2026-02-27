using CryptoAiBot.Core.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CryptoAiBot.Infrastructure.Services;

public sealed class ExchangeSyncWorker : BackgroundService
{
    private readonly IEnumerable<IExchangeConnector> _connectors;
    private readonly ILogger<ExchangeSyncWorker> _logger;

    public ExchangeSyncWorker(IEnumerable<IExchangeConnector> connectors, ILogger<ExchangeSyncWorker> logger)
    {
        _connectors = connectors;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (var connector in _connectors)
        {
            _ = Task.Run(async () =>
            {
                await connector.StartUserDataStreamAsync(snapshot =>
                {
                    _logger.LogInformation("{Exchange} snapshot @ {Timestamp} totalUsd={TotalUsd}", snapshot.Exchange, snapshot.Timestamp, snapshot.TotalUsdValue);
                    return Task.CompletedTask;
                }, stoppingToken);
            }, stoppingToken);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
