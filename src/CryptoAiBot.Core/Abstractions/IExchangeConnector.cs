using CryptoAiBot.Core.Domain;

namespace CryptoAiBot.Core.Abstractions;

public interface IExchangeConnector
{
    ExchangeType ExchangeType { get; }
    Task<AccountSnapshot> GetAccountSnapshotAsync(CancellationToken cancellationToken = default);
    Task StartUserDataStreamAsync(Func<AccountSnapshot, Task> onSnapshot, CancellationToken cancellationToken = default);
}
