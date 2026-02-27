namespace CryptoAiBot.Core.Domain;

public sealed class TradingSignal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Symbol { get; set; }
    public required string StrategyName { get; set; }
    public TradeHorizon Horizon { get; set; }
    public decimal? EntryPrice { get; set; }
    public decimal? StopLoss { get; set; }
    public decimal? TakeProfit { get; set; }
    public string Narrative { get; set; } = string.Empty;
    public SignalTier MinimumTier { get; set; } = SignalTier.Free;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
