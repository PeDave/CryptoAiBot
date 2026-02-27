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
        var query = _dbContext.Signals.AsNoTracking();

        query = userTier switch
        {
            SignalTier.ProPlus => query,
            SignalTier.Pro => query.Where(signal => signal.CreatedAt <= now - ProDelay),
            _ => query.Where(signal => signal.CreatedAt <= now - FreeDelay)
        };

        var visible = await query.ToListAsync(cancellationToken);

        return visible
            .Where(signal => signal.MinimumTier <= userTier)
            .OrderByDescending(signal => signal.CreatedAt)
            .Take(100)
            .ToArray();
    }
}
