namespace ConfigurationRepository.Samples.VaultWithDbWebApp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VaultSharp.Extensions.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public sealed class ReloadableVaultConfiguration
{ }

public sealed class ReloadableDatabaseConfiguration
{ }

/// <summary>
/// Background service to notify about Vault data changes.
/// </summary>
public class ValutConfigurationReloader : BackgroundService
{
    const byte ReloadCheckIntervalSeconds = 5;
    private readonly ILogger? _logger;
    private readonly IEnumerable<VaultConfigurationProvider> _configProviders;

    /// <summary>
    /// Initializes a new instance of the <see cref="VaultChangeWatcher"/> class.
    /// test.
    /// </summary>
    /// <param name="configuration">Instance of IConfiguration</param>
    /// <param name="logger">Optional logger provider</param>
    public ValutConfigurationReloader(
        IReloadableConfigurationService<ReloadableVaultConfiguration> configurationService,
        ILogger? logger = null)
    {
        var configurationRoot = (IConfigurationRoot)configurationService.Configuration;
        if (configurationRoot == null)
        {
            throw new NullReferenceException(nameof(configurationRoot));
        }

        _logger = logger;

        _configProviders = configurationRoot.Providers.OfType<VaultConfigurationProvider>().ToList()!;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timers = new Dictionary<int, int>(); // key - index of config provider, value - timer
        var minTime = int.MaxValue;
        var i = 0;
        foreach (var provider in _configProviders)
        {
            var waitForSec = ReloadCheckIntervalSeconds;
            minTime = Math.Min(minTime, waitForSec);
            timers[i] = waitForSec;
            i++;
        }

        if (minTime == int.MaxValue)
            return;

        _logger?.LogInformation("VaultChangeWatcher will use {minTime} seconds interval", minTime);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(minTime), stoppingToken).ConfigureAwait(false);
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                for (var j = 0; j < _configProviders.Count(); j++)
                {
                    var timer = timers[j];
                    timer -= minTime;
                    if (timer <= 0)
                    {
                        _configProviders.ElementAt(j).Load();
                        timers[j] = ReloadCheckIntervalSeconds;
                    }
                    else
                    {
                        timers[j] = timer;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"An exception occurred in {nameof(ValutConfigurationReloader)}");
            }
        }
    }
}
