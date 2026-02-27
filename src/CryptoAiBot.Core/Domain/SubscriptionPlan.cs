namespace CryptoAiBot.Core.Domain;

public sealed class SubscriptionPlan
{
    public const string Free = "Free";
    public const string Pro = "Pro";
    public const string ProPlus = "ProPlus";

    public static SignalTier ResolveTier(string? planName) => planName switch
    {
        Pro => SignalTier.Pro,
        ProPlus => SignalTier.ProPlus,
        _ => SignalTier.Free
    };
}
