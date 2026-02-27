using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace CryptoAiBot.Infrastructure.Services;

public sealed class N8nAutomationClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<N8nAutomationClient> _logger;

    public N8nAutomationClient(HttpClient httpClient, ILogger<N8nAutomationClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task TriggerBacktestAsync(string strategyName, IEnumerable<string> symbols, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            strategyName,
            symbols,
            timestamp = DateTimeOffset.UtcNow
        };

        var response = await _httpClient.PostAsJsonAsync("/webhook/backtest", payload, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("n8n backtest trigger sikertelen: {StatusCode}", response.StatusCode);
        }
    }
}
