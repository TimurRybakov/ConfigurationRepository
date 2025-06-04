using VaultSharp;
using VaultSharp.Extensions.Configuration;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.AuthMethods;
using ParametrizedConfiguration;
using ConfigurationRepository.Dapper;
using ConfigurationRepository;
using ConfigurationSampleWebApp;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Get vault connetion string from pre-builded configuration from appsettings.json.
var vaultUri = builder.Configuration.GetConnectionString("Vault")
    ?? throw new Exception("Connection string 'Vault' is not defined.");

// Populate vault with data.
await SaveSecretsToVault(vaultUri);

// Define and build base configuration that will aquire secrets from vault and parametrize itself with values from it.
var baseConfiguration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddVaultConfiguration(
        () => new VaultOptions(
            vaultUri,
            "root"),
        basePath: "configuration_sample_web_api",
        mountPoint: "secret")
    .WithParametrization<ReloadableVaultConfiguration>(builder.Services)
    .Build();

// Get database connection string using parametrized configuration.
var connectionString = baseConfiguration.GetConnectionString("mssql")
    ?? throw new Exception("Connection string 'mssql' is not defined.");

// Populate database with data.
var finalConnectionString = await PopulateDatabaseWithData(baseConfiguration, connectionString);

// Define and build main configuration that includes base and adds parametrized database configuration using Dapper.
var configuration = new ConfigurationBuilder()
    .AddConfiguration(baseConfiguration)
    .AddDapperRepository(
        repository => repository
            .UseDbConnectionFactory(() => new SqlConnection(finalConnectionString))
            .WithSelectConfigurationQuery("select \"Key\", \"Value\" from Configuration"),
        source => source.WithPeriodicalReload())
    .WithParametrization<ReloadableDatabaseConfiguration>(builder.Services)
    .Build();

// Add final configuration as source for application`s configuration.
builder.Configuration.AddConfiguration(configuration);

// Add configuration reloaders.
builder.Services.AddConfigurationReloader(TimeSpan.FromSeconds(5));
builder.Services.AddHostedService<ValutConfigurationReloader>();

// Add services to the container.
builder.Services.AddAuthorization();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapGet("/configuration", (IConfiguration configuration) =>
{
    var valueDictionary = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    foreach (var kvp in configuration.AsEnumerable())
    {
        valueDictionary.Add(kvp.Key, kvp.Value);
    }

    return valueDictionary;
})
.WithName("GetConfiguration")
.WithDescription("Gets all values from current configuration.")
.WithOpenApi();

app.MapPut("/database", async (IConfiguration configuration) =>
{
    var connectionString = configuration.GetConnectionString("mssql")
    ?? throw new Exception("Connection string 'mssql' is not defined.");

    await UpsertConfigurationTable(connectionString);
})
.WithName("SetDatabaseConfiguration")
.WithDescription("Sets database configuration values. Configuration will reload these changes in time.")
.WithOpenApi();

app.MapPut("/vault", async (string? userName, string? password) =>
{
    await SaveSecretsToVault(vaultUri, userName, password);

    return;
})
.WithName("SetVaultSecrets")
.WithDescription("Sets user name and password values in vault. Configuration will reload these changes in time.")
.WithOpenApi();

app.Run();

static async Task SaveSecretsToVault(string vaultUri, string? userName = null, string? password = null)
{
    // Authentication
    IAuthMethodInfo authMethod = new TokenAuthMethodInfo(vaultToken: "root");
    VaultClientSettings vaultClientSettings = new VaultClientSettings(vaultUri, authMethod);
    IVaultClient vaultClient = new VaultClient(vaultClientSettings);

    // Writing a secret
    var secretData = new Dictionary<string, object>
    {
        { "ConnectionStrings:Mssql:UserName", userName ?? "sa" },
        { "ConnectionStrings:Mssql:Password", password ?? "yourStrong(!)Password" }
    };
    await vaultClient.V1.Secrets.KeyValue.V2.WriteSecretAsync(
        path: "configuration_sample_web_api",
        data: secretData,
        mountPoint: "secret"
    );

    Console.WriteLine("Secret written successfully.");
}

static async Task<string> PopulateDatabaseWithData(IConfigurationRoot baseConfiguration, string connectionString)
{
    // Create database if it does not exists. The query will use connection string to master database.
    await EnsureDatabaseCreated(connectionString);

    // Here we update the configuration parameter with created database.
    baseConfiguration["ConnectionStrings:Mssql:DatabaseName"] = "configuration_sample_web_api";

    // Final connection string will use new value of configuration parameter as database name.
    var finalConnectionString = baseConfiguration.GetConnectionString("mssql")
        ?? throw new Exception("Connection string 'mssql' is not defined.");

    // Create configuration table if it does not exists.
    await EnsureConfigurationTableCreated(finalConnectionString);

    // Update database configuration.
    await UpsertConfigurationTable(finalConnectionString);

    return finalConnectionString;
}

static Task EnsureDatabaseCreated(string connectionString)
{
    const string commandText = $"""
            if db_id('configuration_sample_web_api') is null
                create database configuration_sample_web_api
            """;

    return ExecuteCommand(connectionString, commandText);
}

static async Task EnsureConfigurationTableCreated(string connectionString)
{
    const string tablesCommandText = $"""
            if object_id('Configuration', 'U') is null
              create table Configuration
              (
                [Key]   varchar(800) not null primary key clustered,
                [Value] nvarchar(max) null
              );
            """;

    await ExecuteCommand(connectionString, tablesCommandText);
}

static async Task UpsertConfigurationTable(string connectionString)
{
    const string commandText = $"""
            merge Configuration t
            using
            (
                select
                    [Key] = 'CreatedDateTime',
                    [Value] = convert(varchar, getdate(), 121)
                union all
                select
                    [Key] = 'CreatedDateTimeParameter',
                    [Value] = '%CreatedDateTime%'
            ) s on t.[Key] = s.[Key]
            when matched then
                update set [Value] = s.[Value]
            when not matched then
                insert ([Key], [Value])
                values (s.[Key], s.[Value]);
            """;

    await ExecuteCommand(connectionString, commandText);

    Console.WriteLine("Database configuration updated successfully.");
}

static async Task ExecuteCommand(string connectionString, string commandText)
{
    await using var connection = new SqlConnection(connectionString);
    var query = new SqlCommand(commandText, connection);

    await query.Connection.OpenAsync();
    await query.ExecuteNonQueryAsync();
}

public sealed class ReloadableVaultConfiguration
{ }

public sealed class ReloadableDatabaseConfiguration
{ }
