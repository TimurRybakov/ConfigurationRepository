using ConfigurationRepository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace ParametrizedConfiguration;

/// <summary>
/// Provider that provides parametrization of underlying configuration with substitution from it`s own values.
/// <code>
/// IConfiguration _configuration:       ParametrizedConfigurationProvider:
/// {                                    {
///     { "param1", "1+%param2%" },          { "param1", "1+2+3" },
///     { "param2", "2+%param3%" },  -->     { "param2", "2+3" },
///     { "param3", "3" }                    { "param3", "3" }
/// };                                   };
/// </code>
/// </summary>
public class ParametrizedConfigurationProvider : ConfigurationProvider, IReloadableConfigurationProvider
{
    private readonly IDisposable? _changeToken = null;

    private readonly IConfiguration _configuration;

    /// <summary>
    /// The source settings for this provider.
    /// </summary>
    protected ParametrizedConfigurationSource Source { get; }

    /// <summary>
    /// True means that configuration provider will be reloaded periodically by <see cref="ConfigurationReloader"/> service.
    /// </summary>
    public bool PeriodicalReload
    {
        get => Source.PeriodicalReload;
        set => Source.PeriodicalReload = value;
    }

    /// <summary>
    /// <see cref="ParametrizedConfigurationProvider"/> constructor.
    /// </summary>
    /// <param name="source">Parametrized configuration source.</param>
    /// <param name="configuration">A configuration to parametrize.</param>
    public ParametrizedConfigurationProvider(ParametrizedConfigurationSource source, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(configuration);

        Source = source;
        _configuration = configuration;
        _changeToken = ChangeToken.OnChange(Changed, Reload);
    }

    /// <inheritdoc/>
    public override void Load()
    {
        Parametrize();
    }

    /// <inheritdoc/>
    public override void Set(string key, string? value)
    {
        _configuration[key] = value;
        Parametrize();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Virtual overridable method that disposes the object.
    /// </summary>
    /// <param name="disposing">True if called in Dispose() public method.</param>
    protected virtual void Dispose(bool disposing)
    {
        _changeToken?.Dispose();
    }

    /// <summary>
    /// Returns an <see cref="IChangeToken"/> that can be used to observe when parametrizable configuration is reloaded.
    /// </summary>
    /// <returns>An <see cref="IChangeToken"/>.</returns>
    public IChangeToken Changed()
    {
        return _configuration.GetReloadToken();
    }

    /// <inheritdoc/>
    public void Reload()
    {
        Load();
    }

    private void Parametrize()
    {
        Data = _configuration.Parametrize(Source.ParameterPlaceholderOpening, Source.ParameterPlaceholderClosing);
    }

    /// <summary>
    /// Dispose on finalization.
    /// </summary>
    ~ParametrizedConfigurationProvider()
    {
        Dispose(false);
    }
}
