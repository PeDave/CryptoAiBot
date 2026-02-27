using CryptoAiBot.Core.Abstractions;
using CryptoAiBot.Core.Domain;
using CryptoAiBot.Web.Data;
using CryptoAiBot.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CryptoAiBot.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class DashboardController : ControllerBase
{
    private readonly IEnumerable<IExchangeConnector> _connectors;
    private readonly SignalFeedService _signalFeedService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(
        IEnumerable<IExchangeConnector> connectors,
        SignalFeedService signalFeedService,
        UserManager<ApplicationUser> userManager)
    {
        _connectors = connectors;
        _signalFeedService = signalFeedService;
        _userManager = userManager;
    }

    [HttpGet("balances")]
    public async Task<IActionResult> GetBalances(CancellationToken cancellationToken)
    {
        var snapshots = await Task.WhenAll(_connectors.Select(c => c.GetAccountSnapshotAsync(cancellationToken)));
        return Ok(snapshots);
    }

    [HttpGet("signals")]
    public async Task<IActionResult> GetSignals(CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        var tier = SubscriptionPlan.ResolveTier(user?.SubscriptionPlan);
        var signals = await _signalFeedService.GetVisibleSignalsAsync(tier, cancellationToken);
        return Ok(signals);
    }
}
