using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository;

/// <summary>
/// A service that is to be registered in DI-container to mark the underlying configuration for configuration reload service"/>.
/// </summary>
/// <typeparam name="TService"></typeparam>
internal sealed class ReloadableConfigurationService<TService>(
    IConfiguration configuration) : IReloadableConfigurationService<TService>
{

    /// <summary>
    /// Configuration to be reloaded.
    /// </summary>
    public IConfiguration Configuration { get; } = configuration;

    /// <summary>
    /// A type of marker class. Markers may be used by different reloaders.
    /// </summary>
    public Type ServiceType => typeof(TService);
}

/// <summary>
/// Default marker class for reloadable configuration.
/// </summary>
internal sealed class ReloadableConfiguration
{ }
