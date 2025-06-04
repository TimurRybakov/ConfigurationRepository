
using System.Data;
using System.Diagnostics;
using ConfigurationRepository.SqlClient;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ParametrizedConfiguration;

namespace ConfigurationRepository.Tests.Integrational;

internal class SqlClientDictionaryConfigurationRepositoryTests : MsSqlConfigurationRepositoryTests
{
    private const string ConfigurationTableName = "appcfg.Configuration";
    private const string VersionTableName = "appcfg.Version";

    [Test]
    public async Task SqlClient_Repository_Should_Return_Same_Value_As_Saved()
    {
        // Act
        var value = await UpsertConfiguration(DateTime.Now);
        var configuration = new ConfigurationBuilder()
            .AddSqlClientRepository(
                repository => repository
                    .UseConnectionString(ConnectionString)
                    .WithConfigurationTableName(ConfigurationTableName))
            .Build();

        Assert.That(configuration[ConfigurationKey], Is.EqualTo(value));
    }

    [Test]
    public async Task SqlClient_Parametrized_Repository_Should_Return_Same_Value_As_Saved()
    {
        // Act
        var value = await UpsertConfiguration(DateTime.Now);
        var configuration = new ConfigurationBuilder()
            .AddSqlClientRepository(
                repository => repository
                    .UseConnectionString(ConnectionString)
                    .WithConfigurationTableName(ConfigurationTableName))
            .WithParametrization()
            .Build();

        Assert.That(configuration[ConfigurationParametrizedKey], Is.EqualTo(value));
    }

    [TestCase(2)]
    public Task SqlClient_Repository_With_Reloader_Should_Periodically_Reload(int reloadCountShouldBe)
    {
        return RepositoryWithReloaderTest(
            builder =>
            {
                builder
                    .AddSqlClientRepository(
                        repository => repository
                            .UseConnectionString(ConnectionString)
                            .WithConfigurationTableName(ConfigurationTableName),
                        source => source.WithPeriodicalReload());
                return null;
            },
            reloadCountShouldBe,
            key: ConfigurationKey);
    }

    [TestCase(2)]
    public Task SqlClient_Parametrized_Repository_With_Reloader_Should_Periodically_Reload(int reloadCountShouldBe)
    {
        return RepositoryWithReloaderTest(
            builder =>
            {
                builder
                    .AddSqlClientRepository(
                        repository => repository
                            .UseConnectionString(ConnectionString)
                            .WithConfigurationTableName(ConfigurationTableName),
                        source => source.WithPeriodicalReload())
                    .WithParametrization(out var configuration);
                return configuration;
            },
            reloadCountShouldBe,
            key: ConfigurationParametrizedKey);
    }

    [TestCase(1)]
    public Task SqlClient_Versioned_Repository_With_Reloader_Should_Periodically_Reload(int reloadCountShouldBe)
    {
        return RepositoryWithReloaderTest(
            builder =>
            {
                builder
                    .AddSqlClientRepository(
                        repository => repository
                            .UseConnectionString(ConnectionString)
                            .WithConfigurationTableName(ConfigurationTableName)
                            .WithVersionTableName(VersionTableName),
                        source => source.WithPeriodicalReload());
                return null;
            },
            reloadCountShouldBe,
            key: ConfigurationKey);
    }

    [TestCase(1)]
    public Task SqlClient_Parametrized_Versioned_Repository_With_Reloader_Should_Periodically_Reload(int reloadCountShouldBe)
    {
        return RepositoryWithReloaderTest(
            builder =>
            {
                builder
                    .AddSqlClientRepository(
                        repository => repository
                            .UseConnectionString(ConnectionString)
                            .WithConfigurationTableName(ConfigurationTableName)
                            .WithVersionTableName(VersionTableName),
                        source => source.WithPeriodicalReload())
                    .WithParametrization(out var configuration);
                return configuration;
            },
            reloadCountShouldBe,
            key: ConfigurationParametrizedKey);
    }

    protected override async Task<int> UpdateConfigurationWithNoChanges()
    {
        string updateQuery = $"""
            update {ConfigurationTableName} set [Value] = [Value]
            where [Key] = 'CurrentDateTime';
            """;

        await using var connection = new SqlConnection(ConnectionString);
        var query = new SqlCommand(updateQuery, connection);

        await query.Connection.OpenAsync();

        return await query.ExecuteNonQueryAsync();
    }

    protected override async Task<string?> UpsertConfiguration(DateTime value)
    {
        string upsertQuery = $"""
            merge {ConfigurationTableName} t
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

        using var connection = new SqlConnection(ConnectionString);
        connection.InfoMessage += (sender, args) =>
        {
            Debug.WriteLine(args.Message); // Log SQL queries
        };

        using var command = new SqlCommand(upsertQuery, connection);
        command.Parameters.Add("@value", SqlDbType.DateTime).Value = value;

        await command.Connection.OpenAsync();

        return (string?) await command.ExecuteScalarAsync();
    }
}
