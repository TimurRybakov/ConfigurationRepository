
using ConfigurationRepository.Dapper;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ParametrizedConfiguration;

namespace ConfigurationRepository.IntegrationTests;

[TestFixture]
internal class DapperJsonConfigurationRepositoryTests : MsSqlConfigurationRepositoryTests
{
    private const string SelectConfigurationQuery = "select JsonValue as \"Value\" from appcfg.JsonConfiguration where \"Key\" = @Key";
    private const string SelectCurrentVersionQuery = "select Version from appcfg.JsonConfiguration where \"Key\" = @Key";
    private const string ConfigurationRepositoryKey = "AKey";

    [Test]
    public async Task Dapper_Json_Repository_Should_Return_Same_Value_As_Saved()
    {
        // Act
        var value = await UpsertConfiguration(DateTime.Now);
        var configuration = new ConfigurationBuilder()
            .AddDapperJsonRepository(
                repository => repository
                    .UseDbConnectionFactory(() => new SqlConnection(ConnectionString))
                    .WithSelectConfigurationQuery(SelectConfigurationQuery)
                    .WithKey(ConfigurationRepositoryKey))
            .Build();

        Assert.That(configuration[ConfigurationKey], Is.EqualTo(value));
    }

    [Test]
    public async Task Dapper_Parametrized_Json_Repository_Should_Return_Same_Value_As_Saved()
    {
        // Act
        var value = await UpsertConfiguration(DateTime.Now);
        var configuration = new ConfigurationBuilder()
            .AddDapperJsonRepository(
                repository => repository
                    .UseDbConnectionFactory(() => new SqlConnection(ConnectionString))
                    .WithSelectConfigurationQuery(SelectConfigurationQuery)
                    .WithKey(ConfigurationRepositoryKey))
            .WithParametrization()
            .Build();

        Assert.That(configuration[ConfigurationParametrizedKey], Is.EqualTo(value));
    }

    [TestCase(2)]
    public Task Dapper_Json_Repository_With_Reloader_Should_Periodically_Reload(int expectedReloadCount)
    {
        return RepositoryWithReloaderTest(
            builder =>
            {
                builder
                    .AddDapperJsonRepository(
                        repository => repository
                            .UseDbConnectionFactory(() => new SqlConnection(ConnectionString))
                            .WithSelectConfigurationQuery(SelectConfigurationQuery)
                            .WithKey(ConfigurationRepositoryKey),
                        source => source.WithPeriodicalReload());
                return null;
            },
            expectedReloadCount,
            key: ConfigurationKey);
    }

    [TestCase(2)]
    public Task Dapper_Parametrized_Json_Repository_With_Reloader_Should_Periodically_Reload(int expectedReloadCount)
    {
        return RepositoryWithReloaderTest(
            builder =>
            {
                builder
                    .AddDapperJsonRepository(
                        repository => repository
                            .UseDbConnectionFactory(() => new SqlConnection(ConnectionString))
                            .WithSelectConfigurationQuery(SelectConfigurationQuery)
                            .WithKey(ConfigurationRepositoryKey),
                        source => source.WithPeriodicalReload())
                    .WithParametrization(out var configuration);
                return configuration;
            },
            expectedReloadCount,
            key: ConfigurationParametrizedKey);
    }

    [TestCase(1)]
    public Task Dapper_Versioned_Json_Repository_With_Reloader_Should_Periodically_Reload(int expectedReloadCount)
    {
        return RepositoryWithReloaderTest(
            builder =>
            {
                builder
                    .AddDapperJsonRepository(
                        repository => repository
                            .UseDbConnectionFactory(() => new SqlConnection(ConnectionString))
                            .WithSelectConfigurationQuery(SelectConfigurationQuery)
                            .WithSelectCurrentVersionQuery(SelectCurrentVersionQuery)
                            .WithKey(ConfigurationRepositoryKey),
                        source => source.WithPeriodicalReload());
                return null;
            },
            expectedReloadCount,
            key: ConfigurationKey);
    }

    [TestCase(1)]
    public Task Dapper_Parametrized_Versioned_Json_Repository_With_Reloader_Should_Periodically_Reload(int expectedReloadCount)
    {
        return RepositoryWithReloaderTest(
            builder =>
            {
                builder
                    .AddDapperJsonRepository(
                        repository => repository
                            .UseDbConnectionFactory(() => new SqlConnection(ConnectionString))
                            .WithSelectConfigurationQuery(SelectConfigurationQuery)
                            .WithSelectCurrentVersionQuery(SelectCurrentVersionQuery)
                            .WithKey(ConfigurationRepositoryKey),
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
            -- Emulate update that does not happened as value was not changed
            declare @Value nvarchar(max) = (select [JsonValue] from appcfg.JsonConfiguration where [Key] = @Key)
            update appcfg.JsonConfiguration set [JsonValue] = @Value
            where [Key] = @Key
              and hashbytes('SHA2_256', [JsonValue]) != hashbytes('SHA2_256', @Value);
            """;

        await using var connection = new SqlConnection(ConnectionString);

        return await connection.ExecuteAsync(updateQuery, new { Key = ConfigurationRepositoryKey });
    }

    protected override async Task<string?> UpsertConfiguration(DateTime value)
    {
        const string upsertQuery = $"""
            merge appcfg.JsonConfiguration t
            using
            (
                select
                    [Key]       = 'AKey',
                    [JsonValue] =
                        (
                            select
                                [CurrentDateTime] = convert(varchar, @value, 121),
                                [CurrentDateTimeParameter] = '%CurrentDateTime%'
                            for json path, without_array_wrapper
                        )
            ) s on t.[Key] = s.[Key]
            when matched then
                update set [JsonValue] = s.[JsonValue]
            when not matched then
                insert ([Key], [JsonValue])
                values (s.[Key], s.[JsonValue]);
            select [Value] = convert(varchar, @value, 121)
            """;

        await using var connection = new SqlConnection(ConnectionString);

        return (string?) await connection.ExecuteScalarAsync(upsertQuery, new { value });
    }
}
