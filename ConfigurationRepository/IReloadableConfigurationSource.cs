
using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository;

public interface IReloadableConfigurationSource : IConfigurationSource
{
    /// <summary>
    /// True means that configuration provider will be reloaded periodically by <see cref="ConfigurationReloader"/> service
    /// </summary>
    public bool PeriodicalReload { get; set; }
}
