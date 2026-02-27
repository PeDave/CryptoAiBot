using CryptoAiBot.Core.Domain;
using CryptoAiBot.Web.Data;
using CryptoAiBot.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CryptoAiBot.Web.Pages.Signals;

public sealed class IndexModel : PageModel
{
    private readonly SignalFeedService _signalFeedService;

    public IReadOnlyCollection<SignalEntity> Signals { get; private set; } = Array.Empty<SignalEntity>();

    public IndexModel(SignalFeedService signalFeedService)
    {
        _signalFeedService = signalFeedService;
    }

    public async Task OnGet(CancellationToken cancellationToken)
    {
        // TODO: bejelentkezett user alapján tier kezelése (Identity oldalak hozzáadása után)
        Signals = await _signalFeedService.GetVisibleSignalsAsync(SignalTier.Free, cancellationToken);
    }
}
