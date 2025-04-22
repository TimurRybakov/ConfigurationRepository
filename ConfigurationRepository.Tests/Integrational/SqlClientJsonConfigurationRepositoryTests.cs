
using ConfigurationRepository.SqlClient;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository.Tests;

internal class SqlClientJsonConfigurationRepositoryTests : SqlClientConfigurationRepositoryTests
{
    private const string ConfigurationTableName = "appcfg.JsonConfiguration";
    private const string ConfigurationKey = "AKey";
    private const string ConfigurationVersionFieldName = "[Version]";

    [Test]
    public void Repository_Should_ReturnSameValueAsSaved()
    {
        // Act
        var value = UpsertConfiguration();
        var configuration = new ConfigurationBuilder()
            .AddSqlClientJsonRepository(repository =>
            {
                repository
                    .UseConnectionString(ConnectionString)
                    .WithConfigurationTableName(ConfigurationTableName)
                    .WithKey(ConfigurationKey);
            })
            .Build();

        Assert.That(configuration["CurrentDateTime"], Is.EqualTo(value));
    }

    [TestCase(2)]
    public Task RepositoryWithReloader_Should_PeriodicallyReload(int reloadCountShouldBe)
    {
        return RepositoryWithReloaderTest(configureBuilder =>
        {
            configureBuilder.AddSqlClientJsonRepository(
                configureRepository =>
                {
                    configureRepository
                        .UseConnectionString(ConnectionString)
                        .WithConfigurationTableName(ConfigurationTableName)
                        .WithKey(ConfigurationKey);
                },
                source => source.WithPeriodicalReload());
        }, reloadCountShouldBe);
    }

    [TestCase(1)]
    public Task RepositoryWithReloaderAndVersionChecker_Should_PeriodicallyReload(int reloadCountShouldBe)
    {
        return RepositoryWithReloaderTest(configureBuilder =>
        {
            configureBuilder.AddSqlClientJsonRepository(
                configureRepository =>
                {
                    configureRepository
                        .UseConnectionString(ConnectionString)
                        .WithConfigurationTableName(ConfigurationTableName)
                        .WithVersionFieldName(ConfigurationVersionFieldName)
                        .WithKey(ConfigurationKey);
                },
                source => source.WithPeriodicalReload());
        }, reloadCountShouldBe);
    }

    protected override string? UpdateConfigurationWithNoChanges()
    {
        string updateQuery = $"""
            -- Emulate update that does not happened as value was not changed
            declare @Value nvarchar(max) = (select [JsonValue] from {ConfigurationTableName} where [Key] = @Key)
            update {ConfigurationTableName} set [JsonValue] = @Value
            where [Key] = @Key
              and hashbytes('SHA2_256', [JsonValue]) != hashbytes('SHA2_256', @Value);
            """;

        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(updateQuery, connection);
        command.Parameters.AddWithValue("@Key", ConfigurationKey);

        command.Connection.Open();

        return (string?)command.ExecuteScalar();
    }

    protected override string? UpsertConfiguration()
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
                                [CurrentDateTime] = @value
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

        command.Connection.Open();

        return (string?)command.ExecuteScalar();
    }
}
