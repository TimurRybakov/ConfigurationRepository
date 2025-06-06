
using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository;

/// <summary>
/// Common interface for implementtions by all reloadable configuration source classes.
/// </summary>
public interface IReloadableConfigurationSource : IConfigurationSource
{
    /// <summary>
    /// True means that configuration provider will be reloaded periodically by <see cref="ConfigurationReloader"/> service.
    /// </summary>
    bool PeriodicalReload { get; set; }
}
