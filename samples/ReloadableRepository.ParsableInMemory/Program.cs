using ConfigurationRepository;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Define our json configuration data.
var configJsonData = """{"JSON KEY": "JSON VALUE"}""";

// Create repository object with this data.
var repository = new InMemoryJsonRepository(configJsonData);

// Define our parsable json configuration provider with in-memory repository.
// Also configure configuration source that this configuration with be reloadable
// by ConfigurationReloader hosting servce.
builder.Configuration.AddParsableRepository(
    repository,
    configureSource: source => source.WithPeriodicalReload());

// Next call does two things:
// 1. Registers ConfigurationReloader hosted service in service collection.
// This hosted service periodically reloads it`s configuration providers.
// 2. Registers reloadable configuration service with builder.Configuration
// for ConfigurationReloader. This tells ConfigurationReloader wich providers
// he should serve.
// FYI: Reload period of 0.5 second is just for quick sample results.
builder.Services.AddConfigurationReloader(
    builder.Configuration,
    TimeSpan.FromSeconds(0.5));

// Register hosted service ConfigTestService.
builder.Services.AddHostedService<ConfigTestService>();
// Add our repository to service colletion just to be able to
// inject it into constructor of ConfigTestService afterwards.
builder.Services.AddSingleton(x => repository);

var app = builder.Build();

app.Run();

// In memory configuration repository that takes a json and returns
// it to a configuration parser. JsonConfig can be set outside to
// simulate external repository changes.
class InMemoryJsonRepository(string jsonConfig)
    : IConfigurationRepository
{
    public string JsonConfig { get; set; } = jsonConfig;

    public TData GetConfiguration<TData>()
    {
        return (TData)Convert.ChangeType(JsonConfig, typeof(TData));
    }
}

// This hosted service when started demonstrates how configuration
// changes are updated by ConfigurationReloader hosted service.
class ConfigTestService(
    InMemoryJsonRepository repository, IConfiguration configuration) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Return current value from configuration.
        Console.WriteLine(configuration["JSON KEY"]);

        // Change configuration.
        repository.JsonConfig = """{"JSON KEY": "JSON VALUE CHANGED"}""";

        // Wait for reload to happen.
        await Task.Delay(TimeSpan.FromSeconds(1));

        // Return changed value from configuration.
        Console.WriteLine(configuration["JSON KEY"]);

        return;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
