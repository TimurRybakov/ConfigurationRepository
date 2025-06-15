
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConfigurationRepository;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="ConfigurationReloader"/> hosted service to <paramref name="services"/>.
    /// </summary>
    /// <param name="services">A collection of services.</param>
    /// <param name="period">A time span for <see cref="ConfigurationReloader"/> to wait between reloads.</param>
    /// <returns>A collection of services.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection AddConfigurationReloader(
        this IServiceCollection services, TimeSpan? period = null)
    {
        services.AddHostedService<ConfigurationReloader>(serviceProvider =>
        {
            // Collect all reloadable configuration prviders from registered IReloadableConfigurationService base service.
            var providers = serviceProvider
                .GetRequiredService<IReloadableConfigurationService>().Configuration
                .GetReloadableConfigurationProviders()?
                .ToArray() ?? throw new InvalidOperationException($"No reloadable configuration providers of type {nameof(IReloadableConfigurationProvider)} were found.");

            return new ConfigurationReloader(providers, period);
        });
        return services;
    }

    /// <summary>
    /// Adds new <see cref="IReloadableConfigurationService"/> service and it`s generic version to <paramref name="services"/>.
    /// </summary>
    /// <typeparam name="TService">A generic type of a marker class.</typeparam>
    /// <param name="services">A collection of services.</param>
    /// <param name="configuration">A configuration to be bounded with <see cref="IReloadableConfigurationService"/> service.</param>
    /// <returns>A collection of services.</returns>
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

    /// <summary>
    /// Adds <see cref="ConfigurationReloader"/> hosted service to <paramref name="services"/>
    /// with TService marker class.
    /// </summary>
    /// <param name="services">A collection of services.</param>
    /// <param name="configuration">A configuration to be bounded with <see cref="IReloadableConfigurationService"/> service.</param>
    /// <param name="period">A time span for <see cref="ConfigurationReloader"/> to wait between reloads.</param>
    /// <returns>A collection of services.</returns>
    public static IServiceCollection AddConfigurationReloader<TService>(
        this IServiceCollection services,
        IConfiguration configuration,
        TimeSpan? period = null) =>
        services
            .AddReloadableConfigurationService<TService>(configuration)
            .AddConfigurationReloader(period);

    /// <summary>
    /// Adds <see cref="ConfigurationReloader"/> hosted service to <paramref name="services"/>
    /// with default <see cref="ReloadableConfiguration"/> marker class.
    /// </summary>
    /// <param name="services">A collection of services.</param>
    /// <param name="configuration">A configuration to be bounded with <see cref="IReloadableConfigurationService"/> service.</param>
    /// <param name="period">A time span for <see cref="ConfigurationReloader"/> to wait between reloads.</param>
    /// <returns>A collection of services.</returns>
    public static IServiceCollection AddConfigurationReloader(
        this IServiceCollection services,
        IConfiguration configuration,
        TimeSpan? period = null) =>
        services
            .AddConfigurationReloader<ReloadableConfiguration>(configuration, period);

}
