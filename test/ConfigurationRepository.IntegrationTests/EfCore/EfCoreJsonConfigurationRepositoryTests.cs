using ConfigurationRepository.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using ParametrizedConfiguration;

namespace ConfigurationRepository.IntegrationTests;

[TestFixture]
public class EfCoreJsonConfigurationRepositoryTests
{
    private const string Key = "AKey";

    [Test]
    public async Task EfCore_Json_Repository_Should_Return_Same_Value_As_Saved()
    {
        // Arrange
        var options = GetDbContextOptions(nameof(EfCore_Json_Repository_Should_Return_Same_Value_As_Saved));
        await using var context = new RepositoryDbContext(options);
        var repository = new EntryRepository(context);
        var entry = new ConfigurationEntry { Key = Key, Value = """{"Host":"127.0.0.1"}""" };

        // Act
        await repository.AddAsync(entry);
        var savedEntry = context.ConfigurationEntryDbSet.FirstOrDefault();
        var configuration = new ConfigurationBuilder()
            .AddEfCoreJsonRepository(Key, options)
            .Build();

        Assert.That(configuration["Host"], Is.EqualTo("127.0.0.1"));
    }

    [Test]
    public async Task EfCore_Parametrized_Json_Repository_Should_Return_Same_Value_As_Saved()
    {
        // Arrange
        var options = GetDbContextOptions(nameof(EfCore_Parametrized_Json_Repository_Should_Return_Same_Value_As_Saved));
        await using var context = new RepositoryDbContext(options);
        var repository = new EntryRepository(context);
        var entry = new ConfigurationEntry
        {
            Key = Key,
            Value = """{ "localhost" : "127.0.0.1", "host" : "%localhost%" }"""
        };

        // Act
        await repository.AddAsync(entry);
        var savedEntry = context.ConfigurationEntryDbSet.FirstOrDefault();
        var configuration = new ConfigurationBuilder()
            .AddEfCoreJsonRepository(Key, options)
            .WithParametrization()  
            .Build();

        Assert.That(configuration["Host"], Is.EqualTo("127.0.0.1"));
    }

    [Test]
    public async Task EfCore_Json_Repository_With_Reloader_Should_Periodically_Reload()
    {
        // Arrange
        var options = GetDbContextOptions(nameof(EfCore_Json_Repository_With_Reloader_Should_Periodically_Reload));
        await using var context = new RepositoryDbContext(options);
        var repository = new EntryRepository(context);
        var entry = new ConfigurationEntry { Key = Key, Value = """{"Host":"127.0.0.1"}""" };

        // Act
        await repository.AddAsync(entry);
        Console.WriteLine("Configuration saved to repository.");
        var savedEntry = context.ConfigurationEntryDbSet.First();
        var configuration = new ConfigurationBuilder()
            .AddEfCoreJsonRepository(
                Key,
                options,
                source => source.WithPeriodicalReload())
            .Build();

        Assert.That(configuration["Host"], Is.EqualTo("127.0.0.1"));

        ChangeToken.OnChange(
            () => configuration.GetReloadToken(),
            () => Console.WriteLine("Configuration reloaded."));

        var services = new ServiceCollection();
        services.AddConfigurationReloader(configuration, TimeSpan.FromMilliseconds(15));
        var serviceProvider = services.BuildServiceProvider();
        var tcs = new TaskCompletionSource();
        var hostedServices = serviceProvider.GetServices<IHostedService>();
        var reloader = hostedServices.OfType<ConfigurationReloader>().First();
        reloader.OnProvidersReloaded += _ => tcs.TrySetResult();

        savedEntry.Value = """{"Host":"localhost"}""";
        context.SaveChanges();
        Console.WriteLine("Configuration changed.");

        await reloader.StartAsync(CancellationToken.None);
        await tcs.Task; // wait for next reload
        await reloader.StopAsync(CancellationToken.None);

        // Assert
        Assert.That(configuration["Host"], Is.EqualTo("localhost"));
    }

    [Test]
    public async Task EfCore_Parametrized_Json_Repository_With_Reloader_Should_Periodically_Reload()
    {
        // Arrange
        var options = GetDbContextOptions(nameof(EfCore_Parametrized_Json_Repository_With_Reloader_Should_Periodically_Reload));
        await using var context = new RepositoryDbContext(options);
        var repository = new EntryRepository(context);
        var entry = new ConfigurationEntry
        {
            Key = Key,
            Value = """{ "localhost" : "127.0.0.1", "host" : "%localhost%" }"""
        };

        // Act
        await repository.AddAsync(entry);
        Console.WriteLine("Configuration saved to repository.");
        var savedEntry = context.ConfigurationEntryDbSet.First();
        var configuration = new ConfigurationBuilder()
            .AddEfCoreJsonRepository(
                Key,
                options,
                source => source.WithPeriodicalReload())
            .WithParametrization(out var parametrizableConfiguration)
            .Build();

        Assert.That(configuration["Host"], Is.EqualTo("127.0.0.1"));

        ChangeToken.OnChange(
            () => configuration.GetReloadToken(),
            () => Console.WriteLine("Configuration reloaded."));

        var services = new ServiceCollection();
        services.AddConfigurationReloader(parametrizableConfiguration, TimeSpan.FromMilliseconds(15));
        var serviceProvider = services.BuildServiceProvider();
        var tcs = new TaskCompletionSource();
        var hostedServices = serviceProvider.GetServices<IHostedService>();
        var reloader = hostedServices.OfType<ConfigurationReloader>().First();
        reloader.OnProvidersReloaded += _ => tcs.TrySetResult();

        savedEntry.Value = """{ "home" : "192.168.0.1", "host" : "%home%" }""";
        context.SaveChanges();
        Console.WriteLine("Configuration changed.");

        await reloader.StartAsync(CancellationToken.None);
        await tcs.Task; // wait for next reload
        await reloader.StopAsync(CancellationToken.None);

        // Assert
        Assert.That(configuration["Host"], Is.EqualTo("192.168.0.1"));
    }

    private static DbContextOptions<RepositoryDbContext> GetDbContextOptions(string databaseName)
    {
        var options = new DbContextOptionsBuilder<RepositoryDbContext>();
        options
            .UseInMemoryDatabase(databaseName)
            .UseTable(tableName: "testConfiguration");
        return options.Options;
    }
}
