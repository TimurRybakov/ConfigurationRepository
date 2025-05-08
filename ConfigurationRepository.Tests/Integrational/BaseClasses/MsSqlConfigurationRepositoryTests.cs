using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace ConfigurationRepository.Tests.Integrational;

[TestFixture]
internal abstract class MsSqlConfigurationRepositoryTests
{
    private readonly MsSqlContainerSingleton _msSqlContainerSingleton = MsSqlContainerSingleton.Instance.Value;

    protected string ConnectionString { get; private set; }

    [OneTimeSetUp]
    protected void InitOnce()
    {
        ConnectionString = _msSqlContainerSingleton.ConnectionString;
    }

    protected async Task RepositoryWithReloaderTest(
        Action<IConfigurationBuilder> configureBuilder, int reloadCountShouldBe)
    {
        // Act
        var value = await UpsertConfiguration();
        Console.WriteLine("Configuration saved to repository.");
        var configurationBuilder = new ConfigurationBuilder();
        configureBuilder(configurationBuilder);
        var configuration = configurationBuilder.Build();

        var reloadCount = 0;
        ChangeToken.OnChange(
            () => configuration.GetReloadToken(),
            () =>
            {
                Console.WriteLine("Configuration reloaded.");
                reloadCount++;
            });

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddConfigurationRepositoryReloader(TimeSpan.FromMilliseconds(20));
        var serviceProvider = services.BuildServiceProvider();
        var tcs = new TaskCompletionSource();
        var reloader = serviceProvider.GetRequiredService<ConfigurationReloader>();
        reloader.OnProvidersReloaded += _ => tcs.TrySetResult();

        await UpdateConfigurationWithNoChanges();
        Console.WriteLine("Configuration not changed.");

        await reloader.StartAsync(CancellationToken.None);
        await tcs.Task; // wait for next reload

        Assert.That(configuration["CurrentDateTime"], Is.EqualTo(value));

        value = await UpsertConfiguration();
        Console.WriteLine("Configuration changed.");
        tcs = new TaskCompletionSource();
        await tcs.Task; // wait for next reload

        await reloader.StopAsync(CancellationToken.None);

        Assert.That(configuration["CurrentDateTime"], Is.EqualTo(value));
        Assert.That(reloadCount, Is.EqualTo(reloadCountShouldBe), "Number of reloads does not match.");
    }

    protected abstract Task<string?> UpsertConfiguration();

    protected abstract Task<int> UpdateConfigurationWithNoChanges();
}

