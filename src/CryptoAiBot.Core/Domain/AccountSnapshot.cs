namespace CryptoAiBot.Core.Domain;

public sealed record AssetBalance(string Asset, decimal Available, decimal Locked);

public sealed record AccountSnapshot(
    ExchangeType Exchange,
    DateTimeOffset Timestamp,
    decimal TotalUsdValue,
    decimal UnrealizedPnL,
    IReadOnlyCollection<AssetBalance> Balances);
