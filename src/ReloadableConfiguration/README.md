# ReloadableConfiguration
An ASP .NET Core class library for defining reloadable configuration repositories that periodically reload their contents to reflect data changes made by others.

[![NuGet](https://img.shields.io/nuget/dt/ReloadableConfiguration.svg)](https://www.nuget.org/packages/ReloadableConfiguration)
[![NuGet](https://img.shields.io/nuget/vpre/ReloadableConfiguration.svg)](https://www.nuget.org/packages/ReloadableConfiguration)

### Installation:

+ from [NuGet](https://www.nuget.org/packages/ReloadableConfiguration);
+ from package manager console:
```
Install-Package ReloadableConfiguration
```    
+ from command line:
```
dotnet add package ReloadableConfiguration
```

### Usage:

- The source of reloadable configuration repository provider should have `PeriodicalReload` property set to `true`.
This may be done by calling `WithPeriodicalReload()` extension method on source configurator.
- A `ConfigurationReloader` hosted service should be registered in service collection using `AddConfigurationReloader()` extension method.

### Examples:

The following sample explains how to define a configuration repository that is periodically reloaded to update it\`s configuration if provider data changes. A working example you may find in [ReloadableRepository.ParsableInMemory sample project on github](../../samples/ReloadableRepository.ParsableInMemory).
```csharp
using ConfigurationRepository;
using Microsoft.AspNetCore.Builder;

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

var app = builder.Build();

app.Run();

// In memory configuration repository that takes a json and returns
// it to a configuration parser. JsonConfig can be set outside to
// simulate external repository changes. This class just demonstrates
// a minimal parsable repository implementation.
class InMemoryJsonRepository(string jsonConfig)
    : IConfigurationRepository
{
    public string JsonConfig { get; set; } = jsonConfig;

    public TData GetConfiguration<TData>()
    {
        return (TData)Convert.ChangeType(JsonConfig, typeof(TData));
    }
}
```
The complete example you may find in [ReloadableRepository.ParsableInMemory project on github](../../samples/ReloadableRepository.ParsableInMemory).
