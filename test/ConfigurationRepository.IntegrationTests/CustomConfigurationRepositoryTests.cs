using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;

namespace ConfigurationRepository.Tests.Integrational;

[Parallelizable]
[TestFixture]
public class CustomConfigurationRepositoryTests
{
    private InMemoryDictionaryRepository DictionaryRepository { get; set; }

    private InMemoryJsonRepository JsonRepository { get; set; }

    [OneTimeSetUp]
    public void Setup()
    {
        DictionaryRepository = new InMemoryDictionaryRepository(
            new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["Key1"] = "Value1",
                ["Key2"] = "Value2"
            });
        JsonRepository = new InMemoryJsonRepository(
            """
            {
              "Logging": {
                "LogLevel": {
                  "Default": "Information",
                  "Microsoft.AspNetCore": "Warning"
                }
              }
            }
            """);
    }

    [Test]
    public void Dictionary_Repository_Configuration_Should_Returns_Correct_Values()
    {
        var configuration = new ConfigurationBuilder()
            .AddDictionaryRepository(DictionaryRepository)
            .Build();

        Assert.That(configuration["Key1"], Is.EqualTo("Value1"));
    }

    [Test]
    public void Json_Repository_Configuration_Should_Returns_Correct_Values()
    {
        var configuration = new ConfigurationBuilder()
            .AddParsableRepository(JsonRepository)
            .Build();

        Assert.That(configuration["logging:LogLevel:Default"], Is.EqualTo("Information"));
    }

    [Test]
    public async Task Json_Repository_Configuration_With_Reloader_Should_Periodically_Reload()
    {
        var configuration = new ConfigurationBuilder()
            .AddParsableRepository(JsonRepository, configureSource: source => source.WithPeriodicalReload())
            .Build();

        var reloadCount = 0;
        using var _ = ChangeToken.OnChange(
            () => configuration.GetReloadToken(),
            () =>
            {
                Console.WriteLine("Configuration reloaded.");
                reloadCount++;
            });

        var services = new ServiceCollection();
        services.AddConfigurationReloader(configuration, TimeSpan.FromMilliseconds(20));
        var serviceProvider = services.BuildServiceProvider();
        var tcs = new TaskCompletionSource();
        var hostedServices = serviceProvider.GetServices<IHostedService>();
        var reloader = hostedServices.OfType<ConfigurationReloader>().First();
        reloader.OnProvidersReloaded += _ => tcs.TrySetResult();
        await reloader.StartAsync(CancellationToken.None);
        try
        {
            // Add value to dictionary that is the source for in memory configuration
            JsonRepository.JsonConfig = """
                {
                  "Logging": {
                    "LogLevel": {
                      "Default": "Information",
                      "Microsoft.AspNetCore": "Warning"
                    }
                  },
                  "ReloadTest": "Passed"
                }
                """;
            await tcs.Task; // wait for next reload
            var firstReloadedValue = configuration["ReloadTest"];

            // 4. Update configuration with new value to check reload.
            JsonRepository.JsonConfig = """
                {
                  "Logging": {
                    "LogLevel": {
                      "Default": "Information",
                      "Microsoft.AspNetCore": "Warning"
                    }
                  },
                  "ReloadTest": "Passed again"
                }
                """;
            Console.WriteLine("Configuration changed.");
            tcs = new TaskCompletionSource();
            await tcs.Task; // wait for next reload
            var secondReloadedValue = configuration["ReloadTest"];

            Assert.Multiple(() =>
            {
                Assert.That(firstReloadedValue, Is.EqualTo("Passed"));
                Assert.That(secondReloadedValue, Is.EqualTo("Passed again"));
            });
        }
        finally
        {
            await reloader.StopAsync(CancellationToken.None);
        }
    }

    private sealed class InMemoryDictionaryRepository(IDictionary<string, string?> configuration) : IRepository
    {
        public TData GetConfiguration<TData>() =>
            (TData)configuration;
    }

    private sealed class InMemoryJsonRepository(string jsonConfig) : IRepository
    {
        public string JsonConfig { get; set; } = jsonConfig;

        public TData GetConfiguration<TData>()
        {
            return (TData)Convert.ChangeType(JsonConfig, typeof(TData));
        }
    }
}
