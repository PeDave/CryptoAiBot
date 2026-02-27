using CryptoAiBot.Core.Abstractions;
using CryptoAiBot.Infrastructure.Exchange.Bitget;
using CryptoAiBot.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoAiBot.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BitgetOptions>(configuration.GetSection(BitgetOptions.SectionName));

        services.AddHttpClient<IExchangeConnector, BitgetExchangeConnector>();
        services.AddHttpClient<N8nAutomationClient>(client =>
        {
            var baseUrl = configuration["N8n:BaseUrl"] ?? "http://127.0.0.1:5678";
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddSingleton<ISignalEngine, RuleBasedSignalEngine>();
        services.AddHostedService<ExchangeSyncWorker>();
        return services;
    }
}
