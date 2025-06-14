using System.Data;
using ConfigurationRepository.SqlClient;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ParametrizedConfiguration;

namespace ConfigurationRepository.IntegrationTests;

[TestFixture]
internal class SqlClientJsonConfigurationRepositoryTests : MsSqlConfigurationRepositoryTests
{
    private const string ConfigurationTableName = "appcfg.JsonConfiguration";
    private const string ConfigurationRepositoryKey = "AKey";
    private const string ConfigurationVersionFieldName = "[Version]";
    private const string ConfigurationValueFieldName = "[JsonValue]";

    [Test]
    public async Task SqlClient_Json_Repository_Should_Return_Same_Value_As_Saved()
    {
        // Act
        var value = await UpsertConfiguration(DateTime.Now);
        var configuration = new ConfigurationBuilder()
            .AddSqlClientJsonRepository(
                repository => repository
                    .UseConnectionString(ConnectionString)
                    .WithConfigurationTableName(ConfigurationTableName)
                    .WithValueFieldName(ConfigurationValueFieldName)
                    .WithKey(ConfigurationRepositoryKey))
            .Build();

        Assert.That(configuration[ConfigurationKey], Is.EqualTo(value));
    }

    [Test]
    public async Task SqlClient_Json_Parametrized_Repository_Should_Return_Same_Value_As_Saved()
    {
        // Act
        var value = await UpsertConfiguration(DateTime.Now);
        var configuration = new ConfigurationBuilder()
            .AddSqlClientJsonRepository(
                repository => repository
                    .UseConnectionString(ConnectionString)
                    .WithConfigurationTableName(ConfigurationTableName)
                    .WithValueFieldName(ConfigurationValueFieldName)
                    .WithKey(ConfigurationRepositoryKey))
            .WithParametrization()
            .Build();

        Assert.That(configuration[ConfigurationKey], Is.EqualTo(value));
    }

    [TestCase(2)]
    public Task SqlClient_Json_Repository_With_Reloader_Should_Periodically_Reload(int reloadCountShouldBe)
    {
        return RepositoryWithReloaderTest(
            builder =>
            {
                builder
                    .AddSqlClientJsonRepository(
                        repository => repository
                            .UseConnectionString(ConnectionString)
                            .WithConfigurationTableName(ConfigurationTableName)
                            .WithValueFieldName(ConfigurationValueFieldName)
                            .WithKey(ConfigurationRepositoryKey),
                        source => source.WithPeriodicalReload());
                return null;
            },
            reloadCountShouldBe,
            key: ConfigurationKey);
    }

    [TestCase(2)]
    public Task SqlClient_Json_Parametrized_Repository_With_Reloader_Should_Periodically_Reload(int expectedReloadCount)
    {
        return RepositoryWithReloaderTest(
            builder =>
            {
                builder
                    .AddSqlClientJsonRepository(
                        repository => repository
                            .UseConnectionString(ConnectionString)
                            .WithConfigurationTableName(ConfigurationTableName)
                            .WithValueFieldName(ConfigurationValueFieldName)
                            .WithKey(ConfigurationRepositoryKey),
                        source => source.WithPeriodicalReload())
                    .WithParametrization(out var configuration);
                return configuration;
            },
            expectedReloadCount,
            key: ConfigurationParametrizedKey);
    }

    [TestCase(1)]
    public Task SqlClient_Json_Versioned_Repository_With_Reloader_Should_Periodically_Reload(int expectedReloadCount)
    {
        return RepositoryWithReloaderTest(
            builder =>
            {
                builder
                    .AddSqlClientJsonRepository(
                        repository => repository
                            .UseConnectionString(ConnectionString)
                            .WithConfigurationTableName(ConfigurationTableName)
                            .WithValueFieldName(ConfigurationValueFieldName)
                            .WithVersionFieldName(ConfigurationVersionFieldName)
                            .WithKey(ConfigurationRepositoryKey),
                        source => source.WithPeriodicalReload());
                return null;
            },
            expectedReloadCount,
            key: ConfigurationKey);
    }

    [TestCase(1)]
    public Task SqlClient_Json_Parametrized_Versioned_Repository_With_Reloader_Should_Periodically_Reload(int expectedReloadCount)
    {
        return RepositoryWithReloaderTest(
            builder =>
            {
                builder
                    .AddSqlClientJsonRepository(
                        repository => repository
                            .UseConnectionString(ConnectionString)
                            .WithConfigurationTableName(ConfigurationTableName)
                            .WithValueFieldName(ConfigurationValueFieldName)
                            .WithVersionFieldName(ConfigurationVersionFieldName)
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
        string updateQuery = $"""
            -- Emulate update that will not happen as value was not changed
            declare @Value nvarchar(max) = (select [JsonValue] from {ConfigurationTableName} where [Key] = @Key)
            update {ConfigurationTableName} set [JsonValue] = @Value
            where [Key] = @Key
              and hashbytes('SHA2_256', [JsonValue]) != hashbytes('SHA2_256', @Value);
            """;

        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(updateQuery, connection);
        command.Parameters.AddWithValue("@Key", ConfigurationRepositoryKey);

        await command.Connection.OpenAsync();

        return await command.ExecuteNonQueryAsync();
    }

    protected override async Task<string?> UpsertConfiguration(DateTime value)
    {
        string upsertQuery = $"""
            merge {ConfigurationTableName} t
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

        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(upsertQuery, connection);
        command.Parameters.Add("@value", SqlDbType.DateTime).Value = value;

        await command.Connection.OpenAsync();

        return (string?) await command.ExecuteScalarAsync();
    }
}
