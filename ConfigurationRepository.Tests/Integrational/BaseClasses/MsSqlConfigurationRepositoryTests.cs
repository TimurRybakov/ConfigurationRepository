using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;

namespace ConfigurationRepository.Tests.Integrational;

[TestFixture]
internal abstract class MsSqlConfigurationRepositoryTests
{
    private readonly MsSqlContainerSingleton _msSqlContainerSingleton = MsSqlContainerSingleton.Instance.Value;

    protected const string ConfigurationKey = "CurrentDateTime";
    protected const string ConfigurationParametrizedKey = "CurrentDateTimeParameter";

    protected string ConnectionString { get; private set; }

    [OneTimeSetUp]
    protected void InitOnce()
    {
        ConnectionString = _msSqlContainerSingleton.ConnectionString;
    }

    protected async Task RepositoryWithReloaderTest(
        Func<IConfigurationBuilder, IConfiguration?> configureCofigurationBuilder,
        int expectedReloadCount,
        string key)
    {
        // 1. Generate value and store it into database.
        var value = await UpsertConfiguration(DateTime.Now);
        Console.WriteLine("Configuration saved to repository.");

        // 2. Build configuration and prepare infrastructure.
        var configurationBuilder = new ConfigurationBuilder();
        var parametrizableConfiguration = configureCofigurationBuilder(configurationBuilder);
        var configuration = configurationBuilder.Build();
        var reloadCount = 0;
        using var _ = ChangeToken.OnChange(
            () => configuration.GetReloadToken(),
            () =>
            {
                Console.WriteLine("Configuration reloaded.");
                reloadCount++;
            });
        var services = new ServiceCollection();

        services.AddConfigurationReloader(parametrizableConfiguration ?? configuration, TimeSpan.FromMilliseconds(20));
        var serviceProvider = services.BuildServiceProvider();
        var tcs = new TaskCompletionSource();
        var hostedServices = serviceProvider.GetServices<IHostedService>();
        var reloader = hostedServices.OfType<ConfigurationReloader>().First();
        reloader.OnProvidersReloaded += _ => tcs.TrySetResult();

        // 3. Update configuration with same value to check that no reload will happen on versioned repository.
        await UpdateConfigurationWithNoChanges();
        Console.WriteLine("Configuration not changed.");
        await reloader.StartAsync(CancellationToken.None);
        try
        {
            await tcs.Task; // wait for next reload
            Assert.That(configuration[key], Is.EqualTo(value));

            // 4. Update configuration with new value to check reload.
            value = await UpsertConfiguration(DateTime.Now);
            Console.WriteLine("Configuration changed.");
            tcs = new TaskCompletionSource();
            await tcs.Task; // wait for next reload
        }
        finally
        {
            await reloader.StopAsync(CancellationToken.None);
        }

        Assert.Multiple(() =>
        {
            Assert.That(configuration[key], Is.EqualTo(value));
            Assert.That(reloadCount, Is.EqualTo(expectedReloadCount), "Number of reloads does not match.");
        });
    }

    protected abstract Task<string?> UpsertConfiguration(DateTime value);

    protected abstract Task<int> UpdateConfigurationWithNoChanges();
}

