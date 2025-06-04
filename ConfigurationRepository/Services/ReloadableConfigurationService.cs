using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository;

internal sealed class ReloadableConfigurationService<TService> : IReloadableConfigurationService<TService>
{
    public ReloadableConfigurationService(IConfiguration configuration) =>
        Configuration = configuration;

    public IConfiguration Configuration { get; }

    public Type ServiceType => typeof(TService);
}

/// <summary>
/// Default marker class for reloadable configuration.
/// </summary>
internal sealed class ReloadableConfiguration
{ }
