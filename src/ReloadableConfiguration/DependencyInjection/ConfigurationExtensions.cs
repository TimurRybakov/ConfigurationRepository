using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository;

internal static class ConfigurationExtensions
{
    /// <summary>
    /// Get all instances of <see cref="IReloadableConfigurationProvider"/> from <paramref name="configuration"/>.
    /// </summary>
    public static IEnumerable<IReloadableConfigurationProvider>? GetReloadableConfigurationProviders(
        this IConfiguration configuration) =>
        (configuration as IConfigurationRoot)?.Providers
        .OfType<IReloadableConfigurationProvider>()
        .Where(x => x.PeriodicalReload);
}
