
using ConfigurationRepository.SqlClient;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace ConfigurationRepository.Tests;

internal class SqlClientConfigurationRepositoryTests
{
    private const string ConnectionString = "Server=DESKTOP-S5G1QVL\\SQL2019;Database=test;Integrated Security=True;Trust Server Certificate=True;";
    private const string ConfigurationTableName = "appcfg.Configuration";
    private const string VersionTableName = "appcfg.Version";

    [Test]
    public void Repository_Should_ReturnSameValueAsSaved()
    {
        // Act
        var value = UpsertConfiguration();
        var configuration = new ConfigurationBuilder()
            .AddSqlClientRepository(repository =>
            {
                repository
                    .UseConnectionString(ConnectionString)
                    .WithConfigurationTableName(ConfigurationTableName);
            })
            .Build();

        Assert.That(configuration["CurrentDateTime"], Is.EqualTo(value));
    }

    [Test]
    public Task RepositoryWithReloader_Should_PeriodicallyReload()
    {
        return RepositoryWithReloaderTest(repository =>
        {
            repository
                .UseConnectionString(ConnectionString)
                .WithConfigurationTableName(ConfigurationTableName);
        });
    }

    [Test]
    public Task RepositoryWithReloaderAndVersionChecker_Should_PeriodicallyReload()
    {
        return RepositoryWithReloaderTest(repository =>
        {
            repository
                .UseConnectionString(ConnectionString)
                .WithConfigurationTableName(ConfigurationTableName)
                .WithVersionTableName(VersionTableName);
        });
    }

    private async Task RepositoryWithReloaderTest(
        Action<SqlClientConfigurationRepository> configureRepository)
    {
        // Act
        var value = UpsertConfiguration();
        Console.WriteLine("Configuration saved to repository.");
        var configuration = new ConfigurationBuilder()
            .AddSqlClientRepository(
                configureRepository,
                source => source.WithPeriodicalReload())
            .Build();

        ChangeToken.OnChange(
            () => configuration.GetReloadToken(),
            () => Console.WriteLine("Configuration reloaded."));

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddConfigurationRepositoryReloader(TimeSpan.FromMilliseconds(1));
        var serviceProvider = services.BuildServiceProvider();
        var tcs = new TaskCompletionSource();
        var reloader = serviceProvider.GetRequiredService<ConfigurationReloader>();
        reloader.OnProvidersReloaded += _ => tcs.SetResult();

        UpdateConfigurationWithNoChanges();
        Console.WriteLine("Configuration not changed.");

        await reloader.StartAsync(CancellationToken.None);
        await tcs.Task; // wait for next reload

        Assert.That(configuration["CurrentDateTime"], Is.EqualTo(value));

        value = UpsertConfiguration();
        Console.WriteLine("Configuration changed.");
        tcs = new TaskCompletionSource();
        await tcs.Task; // wait for next reload

        await reloader.StopAsync(CancellationToken.None);

        Assert.That(configuration["CurrentDateTime"], Is.EqualTo(value));
    }

    private string? UpdateConfigurationWithNoChanges()
    {
        string updateQuery = $"""
            update {ConfigurationTableName} set [Value] = [Value] where [Key] = 'CurrentDateTime';
            """;

        using var connection = new SqlConnection(ConnectionString);
        var query = new SqlCommand(updateQuery, connection);

        query.Connection.Open();

        return (string?)query.ExecuteScalar();
    }

    private string? UpsertConfiguration()
    {
        string upsertQuery = $"""
            declare @value varchar(255) = convert(varchar(255), getdate(), 121);
            merge {ConfigurationTableName} t
            using
            (
                select
                    [Key] = 'CurrentDateTime',
                    [Value] = @value
            ) s on t.[Key] = s.[Key]
            when matched then
                update set [Value] = s.[Value]
            when not matched then
                insert ([Key], [Value])
                values (s.[Key], s.[Value]);
            select [Value] = @value
            """;

        using var connection = new SqlConnection(ConnectionString);
        var query = new SqlCommand(upsertQuery, connection);

        query.Connection.Open();

        return (string?)query.ExecuteScalar();
    }
}
