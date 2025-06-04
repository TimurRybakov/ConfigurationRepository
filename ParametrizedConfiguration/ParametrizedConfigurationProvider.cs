using ConfigurationRepository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace ParametrizedConfiguration;

public class ParametrizedConfigurationProvider : ConfigurationProvider, IReloadableConfigurationProvider
{
    private readonly IDisposable? _changeToken = null;

    private readonly IConfiguration _configuration;

    /// <summary>
    /// The source settings for this provider.
    /// </summary>
    protected ParametrizedConfigurationSource Source { get; }

    /// <summary>
    /// True means that configuration provider will be reloaded periodically by <see cref="ConfigurationReloader"/> service
    /// </summary>
    public bool PeriodicalReload
    {
        get => Source.PeriodicalReload;
        set => Source.PeriodicalReload = value;
    }

    public ParametrizedConfigurationProvider(ParametrizedConfigurationSource source, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(configuration);

        Source = source;
        _configuration = configuration;
        _changeToken = ChangeToken.OnChange(Changed, Reload);
    }

    public override void Load()
    {
        Data = _configuration.Parametrize();
    }

    public override void Set(string key, string? value)
    {
        _configuration[key] = value;
        Data = _configuration.Parametrize();
    }

    public override bool TryGet(string key, out string? value)
    {
        return base.TryGet(key, out value);
    }


    public void Dispose() => Dispose(true);

    protected virtual void Dispose(bool disposing)
    {
        _changeToken?.Dispose();
    }

    public IChangeToken Changed()
    {
        return _configuration.GetReloadToken();
    }

    public void Reload()
    {
        Load();
    }

    ~ParametrizedConfigurationProvider()
    {
        Dispose(false);
    }
}
