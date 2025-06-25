using Microsoft.Extensions.Hosting;

namespace ConfigurationRepository;

/// <summary>
/// A hosted servce that periodically reloads all providers managed by it.
/// </summary>
/// <inheritdoc/>
public sealed class ConfigurationReloader(
    ICollection<IReloadableConfigurationProvider> providers,
    TimeSpan? period = null) : BackgroundService
{
    private readonly PeriodicTimer _timer = new (period ?? TimeSpan.FromSeconds(30));

    /// <summary>
    /// An event that occurs after all providers were reloaded.
    /// </summary>
    public event Action<ConfigurationReloader>? OnProvidersReloaded;

    /// <inheritdoc/>
    public override void Dispose()
    {
        _timer.Dispose();   
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _timer.WaitForNextTickAsync(stoppingToken))
        {
            ReloadProviders();
        }
    }

    private void ReloadProviders()
    {
        foreach (var provider in providers)
        {
            provider.Reload();
        }
        OnProvidersReloaded?.Invoke(this);
    }
}
