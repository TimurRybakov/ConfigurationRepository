namespace ConfigurationRepository;

public sealed class ConfigurationReloader : IDisposable
{
    private readonly ICollection<ConfigurationRepositoryProvider> _providers;
    private readonly Timer _timer;

    public ConfigurationReloader(
        ICollection<ConfigurationRepositoryProvider> providers,
        TimeSpan? period = null,
        TimeSpan? dueTime = null)
    {
        _providers = providers;
        _timer = new Timer(ReloadProviders, null, dueTime ?? TimeSpan.Zero, period ?? TimeSpan.FromSeconds(30));
    }

    public void Dispose()
    {
        _timer.Dispose();
    }

    private void ReloadProviders(object? state)
    {
        foreach (var provider in _providers)
        {
            provider.Reload();
        }
    }
}
