using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository;

/// <summary>
/// Abstraction for configuration service that provides access to reloadable configuration.
/// </summary>
public interface IReloadableConfigurationService
{
    /// <summary>
    /// Configuration to be reloaded.
    /// </summary>
    IConfiguration Configuration { get; }
}

/// <summary>
/// Same abstration that is marked by a generic type.
/// </summary>
/// <typeparam name="TService">A marker type.</typeparam>
public interface IReloadableConfigurationService<in TService> : IReloadableConfigurationService
{ }
