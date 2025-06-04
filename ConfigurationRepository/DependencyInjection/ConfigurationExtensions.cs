using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository;

internal static class ConfigurationExtensions
{
    /// <summary>
    /// Get all <see cref="IReloadableConfigurationProvider"/> instances
    /// </summary>
    public static IEnumerable<IReloadableConfigurationProvider>? GetReloadableConfigurationProviders(
        this IConfiguration configuration) =>
        (configuration as IConfigurationRoot)?.Providers
        .OfType<IReloadableConfigurationProvider>()
        .Where(x => x.PeriodicalReload);
}
