using ConfigurationRepository.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace ConfigurationRepository.Tests.Integrational;

public class EfCoreDictionaryConfigurationRepositoryTests
{
    [Test]
    public async Task Configuration_Should_ReturnSameValueAsSavedByEfCoreRepository()
    {
        // Arrange
        var options = GetDbContextOptions(nameof(Configuration_Should_ReturnSameValueAsSavedByEfCoreRepository));
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
    public async Task EfCoreRepositoryWithReloader_Should_PeriodicallyReload()
    {
        // Arrange
        var options = GetDbContextOptions(nameof(EfCoreRepositoryWithReloader_Should_PeriodicallyReload));
        await using var context = new RepositoryDbContext(options);
        var repository = new EntryRepository(context);
        var entry = new ConfigurationEntry { Key = "Host", Value = "127.0.0.1" };

        // Act
        await repository.AddAsync(entry);
        Console.WriteLine("Configuration saved to repository.");
        var savedEntry = context.ConfigurationEntryDbSet.First();
        var configuration = new ConfigurationBuilder()
            .AddEfCoreRepository(options, source =>
            {
                //source.UseRepositoryChangesNotifier();
                source.WithPeriodicalReload();
            })
            .Build();

        Assert.That(configuration["Host"], Is.EqualTo("127.0.0.1"));

        ChangeToken.OnChange(
            () => configuration.GetReloadToken(),
            () => Console.WriteLine("Configuration reloaded."));

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddConfigurationRepositoryReloader(TimeSpan.FromMilliseconds(15));
        var serviceProvider = services.BuildServiceProvider();
        var tcs = new TaskCompletionSource();
        var reloader = serviceProvider.GetRequiredService<ConfigurationReloader>();
        reloader.OnProvidersReloaded += _ => tcs.TrySetResult();

        savedEntry.Value = "localhost";
        context.SaveChanges();
        Console.WriteLine("Configuration changed.");

        await reloader.StartAsync(CancellationToken.None);
        await tcs.Task; // wait for next reload
        await reloader.StopAsync(CancellationToken.None);

        // Assert
        Assert.That(configuration["Host"], Is.EqualTo(savedEntry?.Value));
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
