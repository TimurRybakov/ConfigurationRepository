
using ConfigurationRepository.SqlClient;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository.Tests;

internal class SqlClientDictionaryConfigurationRepositoryTests : SqlClientConfigurationRepositoryTests
{
    private const string ConfigurationTableName = "appcfg.Configuration";
    private const string VersionTableName = "appcfg.Version";

    [Test]
    public void Repository_Should_ReturnSameValueAsSaved()
    {
        // Act
        var value = UpsertConfiguration();
        var configuration = new ConfigurationBuilder()
            .AddSqlClientDictionaryRepository(repository =>
            {
                repository
                    .UseConnectionString(ConnectionString)
                    .WithConfigurationTableName(ConfigurationTableName);
            })
            .Build();

        Assert.That(configuration["CurrentDateTime"], Is.EqualTo(value));
    }

    [TestCase(2)]
    public Task RepositoryWithReloader_Should_PeriodicallyReload(int reloadCountShouldBe)
    {
        return RepositoryWithReloaderTest(configureBuilder =>
        {
            configureBuilder.AddSqlClientDictionaryRepository(
                configureRepository =>
                {
                    configureRepository
                        .UseConnectionString(ConnectionString)
                        .WithConfigurationTableName(ConfigurationTableName);
                },
                source => source.WithPeriodicalReload());
        }, reloadCountShouldBe);
    }

    [TestCase(1)]
    public Task RepositoryWithReloaderAndVersionChecker_Should_PeriodicallyReload(int reloadCountShouldBe)
    {
        return RepositoryWithReloaderTest(configureBuilder =>
        {
            configureBuilder.AddSqlClientDictionaryRepository(
                configureRepository =>
                {
                    configureRepository
                        .UseConnectionString(ConnectionString)
                        .WithConfigurationTableName(ConfigurationTableName)
                        .WithVersionTableName(VersionTableName);
                },
                source => source.WithPeriodicalReload());
        }, reloadCountShouldBe);
    }

    protected override string? UpdateConfigurationWithNoChanges()
    {
        string updateQuery = $"""
            update {ConfigurationTableName} set [Value] = [Value]
            where [Key] = 'CurrentDateTime';
            """;

        using var connection = new SqlConnection(ConnectionString);
        var query = new SqlCommand(updateQuery, connection);

        query.Connection.Open();

        return (string?)query.ExecuteScalar();
    }

    protected override string? UpsertConfiguration()
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
