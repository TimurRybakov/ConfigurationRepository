using Microsoft.Extensions.Hosting;

namespace ConfigurationRepository;

public sealed class ConfigurationReloader : BackgroundService
{
    private readonly ICollection<IConfigurationRepositoryProvider> _providers;
    private readonly PeriodicTimer _timer;

    public ConfigurationReloader(
        ICollection<IConfigurationRepositoryProvider> providers,
        TimeSpan? period = null)
    {
        _providers = providers;
        _timer = new PeriodicTimer(period ?? TimeSpan.FromSeconds(30));
    }

    public event Action<ConfigurationReloader>? OnProvidersReloaded;

    public override void Dispose()
    {
        _timer.Dispose();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _timer.WaitForNextTickAsync(stoppingToken))
        {
            ReloadProviders();
        }
    }

    private void ReloadProviders()
    {
        foreach (var provider in _providers)
        {
            provider.Reload();
        }
        OnProvidersReloaded?.Invoke(this);
    }
}
