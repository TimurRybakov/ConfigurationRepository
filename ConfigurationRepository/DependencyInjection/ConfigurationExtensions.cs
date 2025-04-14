using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository;

internal static class ConfigurationExtensions
{
    /// <summary>
    /// Get all <see cref="ConfigurationRepositoryProvider"/> instances
    /// </summary>
    public static IEnumerable<ConfigurationRepositoryProvider>? GetConfigurationRepositoryProviders(
        this IConfiguration configuration) =>
        (configuration as IConfigurationRoot)?.Providers
        .OfType<ConfigurationRepositoryProvider>()
        .Where(x => x.Source.PeriodicalReload);
}
