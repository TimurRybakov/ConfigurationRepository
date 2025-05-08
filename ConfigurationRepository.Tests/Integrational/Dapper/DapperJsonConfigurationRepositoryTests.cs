
using ConfigurationRepository.Dapper;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository.Tests.Integrational;

[TestFixture]
internal class DapperJsonConfigurationRepositoryTests : MsSqlConfigurationRepositoryTests
{
    private const string SelectConfigurationQuery = "select JsonValue as \"Value\" from appcfg.JsonConfiguration where \"Key\" = @Key";
    private const string SelectCurrentVersionQuery = "select Version from appcfg.JsonConfiguration where \"Key\" = @Key";
    private const string ConfigurationKey = "AKey";

    [Test]
    public async Task DapperJsonRepository_Should_ReturnSameValueAsSaved()
    {
        // Act
        var value = await UpsertConfiguration();
        var configuration = new ConfigurationBuilder()
            .AddDapperJsonRepository(repository =>
            {
                repository
                    .UseDbConnectionFactory(() => new SqlConnection(ConnectionString))
                    .WithSelectConfigurationQuery(SelectConfigurationQuery)
                    .WithKey(ConfigurationKey);
            })
            .Build();

        Assert.That(configuration["CurrentDateTime"], Is.EqualTo(value));
    }

    [TestCase(2)]
    public Task DapperJsonRepositoryWithReloader_Should_PeriodicallyReload(int reloadCountShouldBe)
    {
        return RepositoryWithReloaderTest(builder =>
        {
            builder.AddDapperJsonRepository(
                repository =>
                {
                    repository
                        .UseDbConnectionFactory(() => new SqlConnection(ConnectionString))
                        .WithSelectConfigurationQuery(SelectConfigurationQuery)
                        .WithKey(ConfigurationKey);
                },
                source => source.WithPeriodicalReload());
        }, reloadCountShouldBe);
    }
        
    [TestCase(1)]
    public Task DapperJsonRepositoryWithReloaderAndVersionChecker_Should_PeriodicallyReload(int reloadCountShouldBe)
    {
        return RepositoryWithReloaderTest(builder =>
        {
            builder.AddDapperJsonRepository(
                repository =>
                {
                    repository
                        .UseDbConnectionFactory(() => new SqlConnection(ConnectionString))
                        .WithSelectConfigurationQuery(SelectConfigurationQuery)
                        .WithSelectCurrentVersionQuery(SelectCurrentVersionQuery)
                        .WithKey(ConfigurationKey);
                },
                source => source.WithPeriodicalReload());
        }, reloadCountShouldBe);
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

        using var connection = new SqlConnection(ConnectionString);

        return await connection.ExecuteAsync(updateQuery, new { Key = ConfigurationKey });
    }

    protected override async Task<string?> UpsertConfiguration()
    {
        const string upsertQuery = $"""
            declare @value varchar(255) = convert(varchar(255), getdate(), 121);
            merge appcfg.JsonConfiguration t
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

        return (string?) await connection.ExecuteScalarAsync(upsertQuery);
    }
}
