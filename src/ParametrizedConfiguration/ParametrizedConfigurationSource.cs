
using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository;

/// <summary>
/// A configuration source for creating <see cref="ParametrizedConfigurationProvider"/>.
/// </summary>
public class ParametrizedConfigurationSource : IReloadableConfigurationSource
{
    /// <summary>
    /// Configuration with parameters.
    /// </summary>
    public IConfiguration? Configuration { get; set; }

    /// <summary>
    /// The string that indicates a start of parameter placeholder.
    /// </summary>
    public string ParameterPlaceholderOpening { get; set; } = "%";

    /// <summary>
    /// The string that indicates an end of parameter placeholder.
    /// </summary>
    public string ParameterPlaceholderClosing { get; set; } = "%";

    /// <summary>
    /// True means that configuration provider will be reloaded periodically by <see cref="ConfigurationReloader"/> service.
    /// </summary>
    public bool PeriodicalReload { get; set; } = false;

    /// <inheritdoc/>
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        _ = Configuration ?? throw new NullReferenceException($"{nameof(Configuration)} is not set.");

        return new ParametrizedConfigurationProvider(this, Configuration);
    }
}
