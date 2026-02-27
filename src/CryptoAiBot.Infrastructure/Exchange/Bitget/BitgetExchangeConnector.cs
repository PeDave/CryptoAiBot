using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
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
        if (!HasApiCredentials())
        {
            var pingPayload = await GetServerTimeAsync(cancellationToken);
            _logger.LogInformation("Bitget kapcsolat él. Ping válasz: {Payload}", pingPayload.ToString());
            _logger.LogWarning("Bitget API kulcsok hiányoznak, ezért csak public ping fut. Adj meg ApiKey/SecretKey/Passphrase értékeket a valós account snapshothoz.");

            return new AccountSnapshot(
                ExchangeType.Bitget,
                DateTimeOffset.UtcNow,
                TotalUsdValue: 0,
                UnrealizedPnL: 0,
                Balances: new[] { new AssetBalance("USDT", 0, 0) });
        }

        var response = await SendSignedGetAsync("/api/v2/spot/account/assets", cancellationToken);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
        var balances = ExtractBalances(payload);
        var totalUsd = balances
            .Where(b => b.Asset is "USDT" or "USDC")
            .Sum(b => b.Available + b.Locked);

        _logger.LogInformation("Bitget account snapshot lekérve. Coin darabszám: {Count}", balances.Count);

        return new AccountSnapshot(
            ExchangeType.Bitget,
            DateTimeOffset.UtcNow,
            TotalUsdValue: totalUsd,
            UnrealizedPnL: 0,
            Balances: balances);
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

    private bool HasApiCredentials() =>
        !string.IsNullOrWhiteSpace(_options.ApiKey) &&
        !string.IsNullOrWhiteSpace(_options.SecretKey) &&
        !string.IsNullOrWhiteSpace(_options.Passphrase);

    private async Task<JsonElement> GetServerTimeAsync(CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync("/api/v2/public/time", cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken));
    }

    private async Task<HttpResponseMessage> SendSignedGetAsync(string requestPath, CancellationToken cancellationToken)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        var prehash = $"{timestamp}GET{requestPath}";
        var signature = Sign(prehash, _options.SecretKey);

        using var request = new HttpRequestMessage(HttpMethod.Get, requestPath);
        request.Headers.Add("ACCESS-KEY", _options.ApiKey);
        request.Headers.Add("ACCESS-SIGN", signature);
        request.Headers.Add("ACCESS-TIMESTAMP", timestamp);
        request.Headers.Add("ACCESS-PASSPHRASE", _options.Passphrase);
        request.Headers.Add("locale", "en-US");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return response;
    }

    private static string Sign(string payload, string secretKey)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToBase64String(hash);
    }

    private static IReadOnlyCollection<AssetBalance> ExtractBalances(JsonElement payload)
    {
        if (!payload.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<AssetBalance>();
        }

        var balances = new List<AssetBalance>();
        foreach (var item in data.EnumerateArray())
        {
            var asset = item.GetProperty("coin").GetString();
            if (string.IsNullOrWhiteSpace(asset))
            {
                continue;
            }

            var available = ParseDecimal(item, "available");
            var locked = ParseDecimal(item, "frozen");
            balances.Add(new AssetBalance(asset, available, locked));
        }

        return balances;
    }

    private static decimal ParseDecimal(JsonElement item, string propertyName)
    {
        if (!item.TryGetProperty(propertyName, out var value))
        {
            return 0;
        }

        return value.ValueKind == JsonValueKind.Number
            ? value.GetDecimal()
            : decimal.TryParse(value.GetString(), out var parsed) ? parsed : 0;
    }
}
