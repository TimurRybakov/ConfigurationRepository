
using ConfigurationRepository;
using Microsoft.Extensions.Configuration;

namespace ParametrizedConfiguration;

public class ParametrizedConfigurationSource : IReloadableConfigurationSource
{
    /// <summary>
    /// Configuration with parameters.
    /// </summary>
    public IConfiguration? Configuration { get; set; }

    /// <summary>
    /// True means that configuration provider will be reloaded periodically by <see cref="ConfigurationReloader"/> service.
    /// </summary>
    public bool PeriodicalReload { get; set; } = false;

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        _ = Configuration ?? throw new NullReferenceException($"{nameof(Configuration)} is not set.");

        return new ParametrizedConfigurationProvider(this, Configuration);
    }
}
