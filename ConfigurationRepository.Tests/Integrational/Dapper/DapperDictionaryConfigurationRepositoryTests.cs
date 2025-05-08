
using ConfigurationRepository.Dapper;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository.Tests.Integrational;

[TestFixture]
internal class DapperDictionaryConfigurationRepositoryTests : MsSqlConfigurationRepositoryTests
{
    private const string SelectConfigurationQuery = "select \"Key\", \"Value\" from appcfg.Configuration";
    private const string SelectCurrentVersionQuery = "select top (1) CurrentVersion from appcfg.Version";

    [Test]
    public async Task SqlClientRepository_Should_ReturnSameValueAsSaved()
    {
        // Act
        var value = await UpsertConfiguration();
        var configuration = new ConfigurationBuilder()
            .AddDapperRepository(repository =>
            {
                repository
                    .UseDbConnectionFactory(() => new SqlConnection(ConnectionString))
                    .WithSelectConfigurationQuery(SelectConfigurationQuery);
            })
            .Build();

        Assert.That(configuration["CurrentDateTime"], Is.EqualTo(value));
    }

    [TestCase(2)]
    public Task SqlClientRepositoryWithReloader_Should_PeriodicallyReload(int reloadCountShouldBe)
    {
        return RepositoryWithReloaderTest(builder =>
        {
            builder.AddDapperRepository(
                repository =>
                {
                    repository
                        .UseDbConnectionFactory(() => new SqlConnection(ConnectionString))
                        .WithSelectConfigurationQuery(SelectConfigurationQuery);
                },
                source => source.WithPeriodicalReload());
        }, reloadCountShouldBe);
    }

    [TestCase(1)]
    public Task SqlClientRepositoryWithReloaderAndVersionChecker_Should_PeriodicallyReload(int reloadCountShouldBe)
    {
        return RepositoryWithReloaderTest(builder =>
        {
            builder.AddDapperRepository(
                repository =>
                {
                    repository
                        .UseDbConnectionFactory(() => new SqlConnection(ConnectionString))
                        .WithSelectConfigurationQuery(SelectConfigurationQuery)
                        .WithSelectCurrentVersionQuery(SelectCurrentVersionQuery);
                },
                source => source.WithPeriodicalReload());
        }, reloadCountShouldBe);
    }

    protected override async Task<int> UpdateConfigurationWithNoChanges()
    {
        const string updateQuery = $"""
            update appcfg.Configuration set [Value] = [Value]
            where [Key] = 'CurrentDateTime';
            """;

        using var connection = new SqlConnection(ConnectionString);

        return await connection.ExecuteAsync(updateQuery);
    }

    protected override async Task<string?> UpsertConfiguration()
    {
        const string upsertQuery = $"""
            declare @value varchar(255) = convert(varchar(255), getdate(), 121);
            merge appcfg.Configuration t
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

        return await connection.ExecuteScalarAsync<string?>(upsertQuery);
    }
}
