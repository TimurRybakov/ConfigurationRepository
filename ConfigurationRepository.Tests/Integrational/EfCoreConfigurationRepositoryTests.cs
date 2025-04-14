using ConfigurationRepository.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.Identity.Client.Extensions.Msal;

namespace ConfigurationRepository.Tests;

public class EfCoreConfigurationRepositoryTests
{
    [OneTimeSetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task Configuration_Should_ReturnSameValueAsSavedByRepository()
    {
        // Arrange
        var options = GetDbContextOptions(nameof(Configuration_Should_ReturnSameValueAsSavedByRepository));
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
    public async Task RepositoryWithReloader_Should_PeriodicallyReload()
    {
        // Arrange
        var options = GetDbContextOptions(nameof(RepositoryWithReloader_Should_PeriodicallyReload));
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

        ChangeToken.OnChange(
            () => configuration.GetReloadToken(),
            () => Console.WriteLine("Configuration reloaded."));

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddConfigurationRepositoryReloader(TimeSpan.FromMilliseconds(50));
        var serviceProvider = services.BuildServiceProvider();
        _  = serviceProvider.GetRequiredService<ConfigurationReloader>();

        savedEntry.Value = "localhost";
        context.SaveChanges();
        Console.WriteLine("Configuration changed.");

        Thread.Sleep(75); // wait for reload

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
