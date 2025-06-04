
using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository;

/// <summary>
/// Abstraction for configuration service that provides access to reloadable configuration.
/// </summary>
public interface IReloadableConfigurationService
{
    IConfiguration Configuration { get; }

    Type ServiceType { get; }
}

public interface IReloadableConfigurationService<in TService> : IReloadableConfigurationService
{ }
