using CryptoAiBot.Core.Domain;
using CryptoAiBot.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CryptoAiBot.Web.Services;

public sealed class SignalFeedService
{
    private static readonly TimeSpan FreeDelay = TimeSpan.FromHours(2);
    private static readonly TimeSpan ProDelay = TimeSpan.FromMinutes(15);

    private readonly AppDbContext _dbContext;

    public SignalFeedService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<SignalEntity>> GetVisibleSignalsAsync(SignalTier userTier, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var query = _dbContext.Signals.AsNoTracking().OrderByDescending(s => s.CreatedAt);

        var visible = await query.Where(signal =>
                userTier == SignalTier.ProPlus ||
                (userTier == SignalTier.Pro && signal.CreatedAt <= now - ProDelay) ||
                (userTier == SignalTier.Free && signal.CreatedAt <= now - FreeDelay))
            .Take(100)
            .ToListAsync(cancellationToken);

        return visible.Where(signal => signal.MinimumTier <= userTier).ToArray();
    }
}
