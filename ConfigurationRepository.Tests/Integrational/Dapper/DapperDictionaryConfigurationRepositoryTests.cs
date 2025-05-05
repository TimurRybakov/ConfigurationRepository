
using ConfigurationRepository.Dapper;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository.Tests.Integrational;

internal class DapperDictionaryConfigurationRepositoryTests : ConfigurationRepositoryTestsBase
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
                    .UseDbConnectionFactory(_connectionFactory)
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
                        .UseDbConnectionFactory(_connectionFactory)
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
                        .UseDbConnectionFactory(_connectionFactory)
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

        using var connection = _connectionFactory();

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

        using var connection = _connectionFactory();

        return await connection.ExecuteScalarAsync<string?>(upsertQuery);
    }
}
