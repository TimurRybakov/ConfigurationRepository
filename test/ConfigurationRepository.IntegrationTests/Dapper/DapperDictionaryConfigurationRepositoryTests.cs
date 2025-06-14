
using ConfigurationRepository.Dapper;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ParametrizedConfiguration;

namespace ConfigurationRepository.IntegrationTests;

[TestFixture]
internal class DapperDictionaryConfigurationRepositoryTests : MsSqlConfigurationRepositoryTests
{
    private const string SelectConfigurationQuery = "select \"Key\", \"Value\" from appcfg.Configuration";
    private const string SelectCurrentVersionQuery = "select top (1) CurrentVersion from appcfg.Version";

    [Test]
    public async Task Dapper_Repository_Should_Return_Same_Value_As_Saved()
    {
        // Act
        var value = await UpsertConfiguration(DateTime.Now);
        var configuration = new ConfigurationBuilder()
            .AddDapperRepository(
                repository => repository
                    .UseDbConnectionFactory(() => new SqlConnection(ConnectionString))
                    .WithSelectConfigurationQuery(SelectConfigurationQuery))
            .Build();

        Assert.That(configuration[ConfigurationKey], Is.EqualTo(value));
    }

    [Test]
    public async Task Dapper_Parametrized_Repository_Should_Return_Same_Value_As_Saved()
    {
        // Act
        var value = await UpsertConfiguration(DateTime.Now);
        var configuration = new ConfigurationBuilder()
            .AddDapperRepository(
                repository => repository
                    .UseDbConnectionFactory(() => new SqlConnection(ConnectionString))
                    .WithSelectConfigurationQuery(SelectConfigurationQuery))
            .WithParametrization()
            .Build();

        Assert.That(configuration[ConfigurationParametrizedKey], Is.EqualTo(value));
    }

    [TestCase(2)]
    public Task Dapper_Repository_With_Reloader_Should_Periodically_Reload(int expectedReloadCount)
    {
        return RepositoryWithReloaderTest(
            builder =>
            {
                builder
                    .AddDapperRepository(
                        repository => repository
                            .UseDbConnectionFactory(() => new SqlConnection(ConnectionString))
                            .WithSelectConfigurationQuery(SelectConfigurationQuery),
                        source => source.WithPeriodicalReload());
                return null;
            },
            expectedReloadCount,
            key: ConfigurationKey);
    }

    [TestCase(2)]
    public Task Dapper_Parametrized_Repository_With_Reloader_Should_Periodically_Reload(int expectedReloadCount)
    {
        return RepositoryWithReloaderTest(
            builder =>
            {
                builder
                    .AddDapperRepository(
                        repository => repository
                            .UseDbConnectionFactory(() => new SqlConnection(ConnectionString))
                            .WithSelectConfigurationQuery(SelectConfigurationQuery),
                        source => source.WithPeriodicalReload())
                    .WithParametrization(out var configuration);
                return configuration;
            },
            expectedReloadCount,
            key: ConfigurationParametrizedKey);
    }

    [TestCase(1)]
    public Task Dapper_Versioned_Repository_With_Reloader_Should_Periodically_Reload(int expectedReloadCount)
    {
        return RepositoryWithReloaderTest(
            builder =>
            {
                builder
                    .AddDapperRepository(
                            repository => repository
                                .UseDbConnectionFactory(() => new SqlConnection(ConnectionString))
                                .WithSelectConfigurationQuery(SelectConfigurationQuery)
                                .WithSelectCurrentVersionQuery(SelectCurrentVersionQuery),
                            source => source.WithPeriodicalReload());
                return null;
            },
            expectedReloadCount,
            key: ConfigurationKey);
    }

    [TestCase(1)]
    public Task Dapper_Parametrized_Versioned_Repository_With_Reloader_Should_Periodically_Reload(int expectedReloadCount)
    {
        return RepositoryWithReloaderTest(
            builder =>
            {
                builder
                    .AddDapperRepository(
                        repository => repository
                            .UseDbConnectionFactory(() => new SqlConnection(ConnectionString))
                            .WithSelectConfigurationQuery(SelectConfigurationQuery)
                            .WithSelectCurrentVersionQuery(SelectCurrentVersionQuery),
                        source => source.WithPeriodicalReload())
                    .WithParametrization(out var configuration);
                return configuration;
            },
            expectedReloadCount,
            key: ConfigurationParametrizedKey);
    }

    protected override async Task<int> UpdateConfigurationWithNoChanges()
    {
        const string updateQuery = $"""
            update appcfg.Configuration set [Value] = [Value]
            where [Key] = 'CurrentDateTime';
            """;

        await using var connection = new SqlConnection(ConnectionString);

        return await connection.ExecuteAsync(updateQuery);
    }

    protected override async Task<string?> UpsertConfiguration(DateTime value)
    {
        const string upsertQuery = $"""
            merge appcfg.Configuration t
            using
            (
                select
                    [Key] = 'CurrentDateTime',
                    [Value] = convert(varchar, @value, 121)
                union all
                select
                    [Key] = 'CurrentDateTimeParameter',
                    [Value] = '%CurrentDateTime%'
            ) s on t.[Key] = s.[Key]
            when matched then
                update set [Value] = s.[Value]
            when not matched then
                insert ([Key], [Value])
                values (s.[Key], s.[Value]);
            select [Value] = convert(varchar, @value, 121)
            """;

        await using var connection = new SqlConnection(ConnectionString);

        return await connection.ExecuteScalarAsync<string?>(upsertQuery, new { value });
    }
}
