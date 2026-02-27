namespace CryptoAiBot.Infrastructure.Exchange.Bitget;

public sealed class BitgetOptions
{
    public const string SectionName = "Bitget";
    public string ApiKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string Passphrase { get; set; } = string.Empty;
    public string RestBaseUrl { get; set; } = "https://api.bitget.com";
    public string WebSocketUrl { get; set; } = "wss://ws.bitget.com/v2/ws/private";
}
