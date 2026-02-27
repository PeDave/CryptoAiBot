using CryptoAiBot.Core.Abstractions;
using CryptoAiBot.Infrastructure.Services;
using CryptoAiBot.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoAiBot.Web.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public sealed class AdminController : ControllerBase
{
    private readonly ISignalEngine _signalEngine;
    private readonly AppDbContext _dbContext;
    private readonly N8nAutomationClient _automationClient;

    public AdminController(ISignalEngine signalEngine, AppDbContext dbContext, N8nAutomationClient automationClient)
    {
        _signalEngine = signalEngine;
        _dbContext = dbContext;
        _automationClient = automationClient;
    }

    [HttpPost("generate-signals")]
    public async Task<IActionResult> GenerateSignals([FromBody] string[] symbols, CancellationToken cancellationToken)
    {
        var signals = await _signalEngine.GenerateSignalsAsync(symbols, cancellationToken);

        foreach (var signal in signals)
        {
            _dbContext.Signals.Add(new SignalEntity
            {
                Id = signal.Id,
                Symbol = signal.Symbol,
                StrategyName = signal.StrategyName,
                Horizon = signal.Horizon,
                EntryPrice = signal.EntryPrice,
                StopLoss = signal.StopLoss,
                TakeProfit = signal.TakeProfit,
                Narrative = signal.Narrative,
                MinimumTier = signal.MinimumTier,
                CreatedAt = signal.CreatedAt
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _automationClient.TriggerBacktestAsync("Momentum+MeanReversion", symbols, cancellationToken);
        return Ok(new { inserted = signals.Count });
    }
}
