using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository;

/// <summary>
/// A service that provides the underlying configuration for configuration reload service.
/// </summary>
/// <typeparam name="TService"></typeparam>
internal sealed class ReloadableConfigurationService<TService>(
    IConfiguration configuration) : IReloadableConfigurationService<TService>
{
    /// <summary>
    /// Configuration to be reloaded.
    /// </summary>
    public IConfiguration Configuration { get; } = configuration;
}

/// <summary>
/// Default marker class for reloadable configuration.
/// </summary>
internal sealed class ReloadableConfiguration
{ }
