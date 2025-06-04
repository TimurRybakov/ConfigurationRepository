
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConfigurationRepository;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConfigurationReloader(
        this IServiceCollection services, TimeSpan? period = null)
    {
        services.AddHostedService<ConfigurationReloader>(serviceProvider =>
        {
            var providers = serviceProvider
                .GetRequiredService<IReloadableConfigurationService>().Configuration
                .GetReloadableConfigurationProviders()?
                .ToArray() ?? throw new InvalidOperationException($"No services of type {nameof(IReloadableConfigurationProvider)} found");

            return new ConfigurationReloader(providers, period);
        });
        return services;
    }

    public static IServiceCollection AddConfigurationReloader(
        this IServiceCollection services,
        IConfiguration configuration,
        TimeSpan? period = null) =>
        services
            .AddConfigurationReloader<ReloadableConfiguration>(configuration, period);

    public static IServiceCollection AddConfigurationReloader<TService>(
        this IServiceCollection services,
        IConfiguration configuration,
        TimeSpan? period = null) =>
        services
            .AddReloadableConfigurationService<TService>(configuration)
            .AddConfigurationReloader(period);

    public static IServiceCollection AddReloadableConfigurationService<TService>(
        this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddSingleton<IReloadableConfigurationService<TService>>(
            new ReloadableConfigurationService<TService>(configuration));

        services.AddSingleton<IReloadableConfigurationService>(
            sp => sp.GetRequiredService<IReloadableConfigurationService<TService>>());

        return services;
    }
}
