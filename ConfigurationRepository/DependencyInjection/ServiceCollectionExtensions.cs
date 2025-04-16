
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ConfigurationRepository;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConfigurationRepositoryReloader(
        this IServiceCollection services, TimeSpan? period = null)
    {
        services.AddSingleton(serviceProvider =>
        {
            var providers = serviceProvider
                .GetRequiredService<IConfiguration>()
                .GetConfigurationRepositoryProviders()?
                .ToArray() ?? throw new InvalidOperationException($"No services of type {nameof(ConfigurationRepositoryProvider)} found");
            return new ConfigurationReloader(providers, period);
        });
        return services;
    }
}
