using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CryptoAiBot.Core.Abstractions;
using CryptoAiBot.Core.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CryptoAiBot.Infrastructure.Exchange.Bitget;

public sealed class BitgetExchangeConnector : IExchangeConnector
{
    private readonly HttpClient _httpClient;
    private readonly BitgetOptions _options;
    private readonly ILogger<BitgetExchangeConnector> _logger;

    public BitgetExchangeConnector(HttpClient httpClient, IOptions<BitgetOptions> options, ILogger<BitgetExchangeConnector> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        _httpClient.BaseAddress = new Uri(_options.RestBaseUrl);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public ExchangeType ExchangeType => ExchangeType.Bitget;

    public async Task<AccountSnapshot> GetAccountSnapshotAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Bitget signed REST hívás bevezetése a hivatalos SDK-val (JKorf.Bitget.Net) production környezetben.
        var response = await _httpClient.GetAsync("/api/v2/public/time", cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);

        _logger.LogInformation("Bitget kapcsolat él. Ping válasz: {Payload}", payload.ToString());

        return new AccountSnapshot(
            ExchangeType.Bitget,
            DateTimeOffset.UtcNow,
            TotalUsdValue: 0,
            UnrealizedPnL: 0,
            Balances: new[] { new AssetBalance("USDT", 0, 0) });
    }

    public async Task StartUserDataStreamAsync(Func<AccountSnapshot, Task> onSnapshot, CancellationToken cancellationToken = default)
    {
        // TODO: websocket privát csatorna auth + subscribe account/channel eseményekre.
        _logger.LogInformation("Bitget websocket stream indul (placeholder implementáció).");
        while (!cancellationToken.IsCancellationRequested)
        {
            await onSnapshot(await GetAccountSnapshotAsync(cancellationToken));
            await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
        }
    }
}
