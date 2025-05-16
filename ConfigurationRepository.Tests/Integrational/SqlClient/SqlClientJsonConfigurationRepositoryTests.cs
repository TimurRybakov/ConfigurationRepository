
using ConfigurationRepository.SqlClient;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ParametrizedConfiguration;

namespace ConfigurationRepository.Tests.Integrational;

[TestFixture]
internal class SqlClientJsonConfigurationRepositoryTests : MsSqlConfigurationRepositoryTests
{
    private const string ConfigurationTableName = "appcfg.JsonConfiguration";
    private const string ConfigurationKey = "AKey";
    private const string ConfigurationParametrizedKey = "Date";
    private const string ConfigurationVersionFieldName = "[Version]";
    private const string ConfigurationValueFieldName = "[JsonValue]";

    [Test]
    public async Task SqlClientJsonRepository_Should_ReturnSameValueAsSaved()
    {
        // Act
        var value = await UpsertConfiguration();
        var configuration = new ConfigurationBuilder()
            .AddSqlClientJsonRepository(
                repository => repository
                    .UseConnectionString(ConnectionString)
                    .WithConfigurationTableName(ConfigurationTableName)
                    .WithValueFieldName(ConfigurationValueFieldName)
                    .WithKey(ConfigurationKey))
            .Build();

        Assert.That(configuration["CurrentDateTime"], Is.EqualTo(value));
    }

    [TestCase(2)]
    public Task SqlClientJsonRepositoryWithReloader_Should_PeriodicallyReload(int reloadCountShouldBe)
    {
        return RepositoryWithReloaderTest(
            () => new ConfigurationBuilder()
                .AddSqlClientJsonRepository(
                    repository => repository
                            .UseConnectionString(ConnectionString)
                            .WithConfigurationTableName(ConfigurationTableName)
                            .WithValueFieldName(ConfigurationValueFieldName)
                            .WithKey(ConfigurationKey),
                    source => source.WithPeriodicalReload()),
            reloadCountShouldBe);
    }

    [TestCase(2)]
    public Task SqlClientJsonRepositoryWithParametrizationWithReloader_Should_PeriodicallyReload(int reloadCountShouldBe)
    {
        return RepositoryWithReloaderTest(
            () => new ParametrizedConfigurationBuilder()
                .AddSqlClientJsonRepository(
                    repository => repository
                        .UseConnectionString(ConnectionString)
                        .WithConfigurationTableName(ConfigurationTableName)
                        .WithValueFieldName(ConfigurationValueFieldName)
                        .WithKey(ConfigurationKey),
                    source => source.WithPeriodicalReload()),
            reloadCountShouldBe, ConfigurationParametrizedKey);
    }

    [TestCase(1)]
    public Task SqlClientJsonRepositoryWithReloaderAndVersionChecker_Should_PeriodicallyReload(int reloadCountShouldBe)
    {
        return RepositoryWithReloaderTest(
            () => new ConfigurationBuilder()
                .AddSqlClientJsonRepository(
                    repository => repository
                        .UseConnectionString(ConnectionString)
                        .WithConfigurationTableName(ConfigurationTableName)
                        .WithValueFieldName(ConfigurationValueFieldName)
                        .WithVersionFieldName(ConfigurationVersionFieldName)
                        .WithKey(ConfigurationKey),
                    source => source.WithPeriodicalReload()),
            reloadCountShouldBe);
    }

    [Test]
    public async Task SqlClientJsonRepositoryWithParametrization_Should_ReturnSameValueAsSaved()
    {
        // Act
        var value = await UpsertConfiguration();
        var configuration = new ParametrizedConfigurationBuilder()
            .AddSqlClientJsonRepository(
                repository => repository
                    .UseConnectionString(ConnectionString)
                    .WithConfigurationTableName(ConfigurationTableName)
                    .WithValueFieldName(ConfigurationValueFieldName)
                    .WithKey(ConfigurationKey),
                source => source.WithPeriodicalReload())
            .Build();

        Assert.That(configuration[ConfigurationParametrizedKey], Is.EqualTo(value));
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
        command.Parameters.AddWithValue("@Key", ConfigurationKey);

        await command.Connection.OpenAsync();

        return await command.ExecuteNonQueryAsync();
    }

    protected override async Task<string?> UpsertConfiguration()
    {
        string upsertQuery = $"""
            declare @value varchar(255) = convert(varchar(255), getdate(), 121);
            merge {ConfigurationTableName} t
            using
            (
                select
                    [Key]       = 'AKey',
                    [JsonValue] =
                        (
                            select
                                [CurrentDateTime] = @value,
                                [Date] = '%CurrentDateTime%'
                            for json path, without_array_wrapper
                        )
            ) s on t.[Key] = s.[Key]
            when matched then
                update set [JsonValue] = s.[JsonValue]
            when not matched then
                insert ([Key], [JsonValue])
                values (s.[Key], s.[JsonValue]);
            select [Value] = @value
            """;

        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(upsertQuery, connection);

        await command.Connection.OpenAsync();

        return (string?) await command.ExecuteScalarAsync();
    }
}
