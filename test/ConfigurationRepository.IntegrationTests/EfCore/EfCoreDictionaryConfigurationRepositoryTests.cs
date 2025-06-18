using ConfigurationRepository.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;

namespace ConfigurationRepository.IntegrationTests;

public class EfCoreDictionaryConfigurationRepositoryTests
{
    [Test]
    public async Task EfCore_Repository_Should_Return_Same_Value_As_Saved()
    {
        // Arrange
        var options = GetDbContextOptions(nameof(EfCore_Repository_Should_Return_Same_Value_As_Saved));
        await using var context = new RepositoryDbContext(options);
        var repository = new EntryRepository(context);
        var entry = new ConfigurationEntry { Key = "Host", Value = "127.0.0.1" };

        // Act
        await repository.AddAsync(entry);
        var savedEntry = context.ConfigurationEntryDbSet.FirstOrDefault();
        var configuration = new ConfigurationBuilder()
            .AddEfCoreRepository(options)
            .Build();

        Assert.That(configuration["Host"], Is.EqualTo(savedEntry?.Value));
    }

    [Test]
    public async Task EfCore_Parametrized_Repository_Should_Return_Same_Value_As_Saved()
    {
        // Arrange
        var options = GetDbContextOptions(nameof(EfCore_Parametrized_Repository_Should_Return_Same_Value_As_Saved));
        await using var context = new RepositoryDbContext(options);
        var repository = new EntryRepository(context);

        // Act
        await repository.AddAsync(new ConfigurationEntry
        {
            Key = "localhost",
            Value = "127.0.0.1"
        });
        await repository.AddAsync(new ConfigurationEntry
        {
            Key = "HostParameter",
            Value = "%LOCALHOST%:8080"
        });

        var configuration = new ConfigurationBuilder()
            .AddEfCoreRepository(options)
            .WithParametrization()
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(configuration["localhost"], Is.EqualTo("127.0.0.1"));
            Assert.That(configuration["HostParameter"], Is.EqualTo("127.0.0.1:8080"));
        });
    }

    [Test]
    public async Task EfCore_Repository_With_Reloader_Should_Periodically_Reload()
    {
        // Arrange
        var options = GetDbContextOptions(nameof(EfCore_Repository_With_Reloader_Should_Periodically_Reload));
        await using var context = new RepositoryDbContext(options);
        var repository = new EntryRepository(context);
        var entry = new ConfigurationEntry { Key = "Key", Value = "Value" };

        // Act
        await repository.AddAsync(entry);
        Console.WriteLine("Configuration saved to repository.");
        var savedEntry = context.ConfigurationEntryDbSet.First();
        var configuration = new ConfigurationBuilder()
            .AddEfCoreRepository(options, source => source.WithPeriodicalReload())
            .Build();

        Assert.That(configuration["Key"], Is.EqualTo("Value"));

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

        savedEntry.Value = "New value";
        context.SaveChanges();
        Console.WriteLine("Configuration changed.");

        await reloader.StartAsync(CancellationToken.None);
        try
        {
            await tcs.Task; // wait for next reload
        }
        finally
        {
            await reloader.StopAsync(CancellationToken.None);
        }

        // Assert
        Assert.That(configuration["Key"], Is.EqualTo(savedEntry.Value));
    }

    [Test]
    public async Task EfCore_Parametrized_Repository_With_Reloader_Should_Periodically_Reload()
    {
        // Arrange
        var options = GetDbContextOptions(nameof(EfCore_Parametrized_Repository_With_Reloader_Should_Periodically_Reload));
        await using var context = new RepositoryDbContext(options);
        var repository = new EntryRepository(context);

        // Act
        await repository.AddAsync(new ConfigurationEntry
        {
            Key = "ConnectionString",
            Value = "<some connection string>"
        });
        await repository.AddAsync(new ConfigurationEntry
        {
            Key = "ExtendedConnectionString",
            Value = "%ConnectionString%(extended)"
        });
        Console.WriteLine("Configuration saved to repository.");
        var savedEntry = context.ConfigurationEntryDbSet.First();
        var configuration = new ConfigurationBuilder()
            .AddEfCoreRepository(options, source => source.WithPeriodicalReload())
            .WithParametrization(out var parametrizableConfiguration)
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(configuration["ConnectionString"], Is.EqualTo("<some connection string>"));
            Assert.That(configuration["ExtendedConnectionString"], Is.EqualTo("<some connection string>(extended)"));
        });

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

        savedEntry.Value = "new connection string";
        context.SaveChanges();
        Console.WriteLine("Configuration changed.");

        await reloader.StartAsync(CancellationToken.None);
        try
        {
            await tcs.Task; // wait for next reload
        }
        finally
        {
            await reloader.StopAsync(CancellationToken.None);
        }

        // Assert
        Assert.That(configuration["ConnectionString"], Is.EqualTo(savedEntry.Value));
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
