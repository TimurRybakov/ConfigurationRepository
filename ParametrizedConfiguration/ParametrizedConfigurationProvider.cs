using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace ParametrizedConfiguration;

public class ParametrizedConfigurationProvider : ConfigurationProvider, IDisposable
{
    private readonly IList<IConfigurationProvider> _providers;
    private readonly IDisposable _changeToken;

    private IDictionary<string, string?> ParametrizableData { get; set; } = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    public ParametrizedConfigurationProvider(IList<IConfigurationProvider> providers)
    {
        _providers = providers;

        _changeToken = ChangeToken.OnChange(
            () => GetReloadToken(),
            () => Load());
    }

    public override void Load()
    {
        var data = ParametrizableData;
        data.Clear();

        foreach (var provider in _providers)
        {
            foreach (var key in provider.GetChildKeys(Enumerable.Empty<string>(), null))
            {
                if (provider.TryGet(key, out string? value))
                {
                    data[key] = value;
                }
            }
        }

        Data = Parametrizer.Parametrize(data);

        //OnReload();
    }

    public override void Set(string key, string? value)
    {
        ParametrizableData[key] = value;
        Load();
    }

    public void Dispose() => Dispose(true);

    protected virtual void Dispose(bool disposing)
    {
        _changeToken?.Dispose();
    }

    private new IChangeToken GetReloadToken()
    {
        var changeTokens = new List<IChangeToken>();
        foreach (var provider in _providers)
        {
            var token = provider.GetReloadToken();
            changeTokens.Add(token);
        }
        return new CompositeChangeToken(changeTokens);
    }

    ~ParametrizedConfigurationProvider()
    {
        Dispose(false);
    }
}
