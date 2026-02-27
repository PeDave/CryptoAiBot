using CryptoAiBot.Core.Domain;

namespace CryptoAiBot.Core.Abstractions;

public interface ISignalEngine
{
    Task<IReadOnlyCollection<TradingSignal>> GenerateSignalsAsync(IEnumerable<string> symbols, CancellationToken cancellationToken = default);
}
