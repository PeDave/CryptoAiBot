using CryptoAiBot.Core.Abstractions;
using CryptoAiBot.Core.Domain;

namespace CryptoAiBot.Infrastructure.Services;

public sealed class RuleBasedSignalEngine : ISignalEngine
{
    public Task<IReadOnlyCollection<TradingSignal>> GenerateSignalsAsync(IEnumerable<string> symbols, CancellationToken cancellationToken = default)
    {
        var signals = symbols.Select(symbol => new TradingSignal
        {
            Symbol = symbol,
            StrategyName = "Momentum+MeanReversion",
            Horizon = TradeHorizon.Short,
            EntryPrice = null,
            StopLoss = null,
            TakeProfit = null,
            MinimumTier = SignalTier.Pro,
            Narrative = $"{symbol}: figyeld a 4H trendet, megerősítés esetén limit beszálló zónát adj meg."
        }).ToArray();

        return Task.FromResult<IReadOnlyCollection<TradingSignal>>(signals);
    }
}
