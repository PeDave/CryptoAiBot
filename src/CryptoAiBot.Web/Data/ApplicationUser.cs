using CryptoAiBot.Core.Domain;
using Microsoft.AspNetCore.Identity;

namespace CryptoAiBot.Web.Data;

public sealed class ApplicationUser : IdentityUser
{
    public string SubscriptionPlan { get; set; } = SubscriptionPlan.Free;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
