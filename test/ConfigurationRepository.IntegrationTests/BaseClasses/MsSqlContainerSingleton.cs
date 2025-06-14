using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace ConfigurationRepository.IntegrationTests;

public partial class MsSqlContainerSingleton : IDisposable
{
    public static readonly Lazy<MsSqlContainerSingleton> Instance =
        new Lazy<MsSqlContainerSingleton>(() => new MsSqlContainerSingleton());

    public readonly MsSqlContainer _mssql = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04")
        .WithName("mssql_configuration_repository")
        .WithReuse(true)
        .WithBindMount($"{Environment.CurrentDirectory}\\tests\\mssql_configuration_repository", "/var/opt/mssql/data")
        .Build();

    public string ConnectionString { get; private set; } = "";

    [GeneratedRegex(@"Database=[^;]+")]
    private static partial Regex DatabaseReplacementRegex();

    private bool disposed = false;

    private MsSqlContainerSingleton()
    {
        Init().GetAwaiter().GetResult();
    }

    ~MsSqlContainerSingleton()
    {
        Dispose(false).GetAwaiter().GetResult();
    }

    private async Task Init()
    {
        await _mssql.StartAsync();

        var connectionString = _mssql.GetConnectionString();

        await EnsureTestDatabaseCreated(connectionString);

        string testConnectionString = DatabaseReplacementRegex().Replace(connectionString, $"Database=test");

        await EnsureConfigurationSchemaCreated(testConnectionString);
        await EnsureConfigurationTablesCreated(testConnectionString);

        ConnectionString = testConnectionString;
    }

    public void Dispose()
    {
        Dispose(true).GetAwaiter().GetResult();
    }

    private async Task Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                await _mssql.DisposeAsync();
            }

            disposed = true;
        }
    }

    private static Task EnsureTestDatabaseCreated(string connectionString)
    {
        const string commandText = $"""
            if db_id('test') is null
                create database test
            """;

        return ExecuteCommand(connectionString, commandText);
    }

    private static Task EnsureConfigurationSchemaCreated(string connectionString)
    {
        const string commandText = $"""
            if schema_id('appcfg') is null
                exec('create schema appcfg authorization dbo');
            """;

        return ExecuteCommand(connectionString, commandText);
    }

    private static async Task EnsureConfigurationTablesCreated(string connectionString)
    {
        const string tablesCommandText = $"""
            if object_id('appcfg.Configuration', 'U') is null
              create table appcfg.Configuration
              (
                [Key]   varchar(800) not null primary key clustered,
                [Value] nvarchar(max) null
              );

            if object_id('appcfg.Version', 'U') is null
              create table appcfg.Version
              (
                [CurrentVersion]   rowversion   not null,
                [PreviousVersion]  varbinary(8)     null
              );

            if not exists(select * from appcfg.Version)
              insert appcfg.Version ([PreviousVersion]) values (null);

            if object_id('appcfg.JsonConfiguration', 'U') is null
              create table appcfg.JsonConfiguration
              (
                [Key]       varchar(255) collate Cyrillic_General_BIN not null primary key clustered,
                [JsonValue] nvarchar(max) null,
                [Version]   rowversion not null
              );
            """;

        const string triggerCommandText = $"""
            create or alter trigger appcfg.tr_Configuration_i_u_d on appcfg.Configuration for insert, update, delete
            as
              set nocount on;

              declare
                @inserted_cs int = isnull((select checksum_agg([checksum]) from (select [checksum] = checksum(*) from inserted) q), 0),
                @deleted_cs  int = isnull((select checksum_agg([checksum]) from (select [checksum] = checksum(*) from deleted) q), 0);

              if @inserted_cs != @deleted_cs
                update appcfg.Version set [PreviousVersion] = [CurrentVersion]; 
            """;

        await ExecuteCommand(connectionString, tablesCommandText);
        await ExecuteCommand(connectionString, triggerCommandText);
    }

    private static async Task ExecuteCommand(string connectionString, string commandText)
    {
        await using var connection = new SqlConnection(connectionString);
        await using var query = new SqlCommand(commandText, connection);

        await query.Connection.OpenAsync();
        await query.ExecuteNonQueryAsync();
    }
}

