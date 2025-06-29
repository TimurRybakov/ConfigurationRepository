# ConfigurationRepository
An ASP .NET Core class library that provides access to application\`s configuration stored in a database or any other repository.

[![NuGet](https://img.shields.io/nuget/dt/ConfigurationRepository.svg)](https://www.nuget.org/packages/ConfigurationRepository)
[![NuGet](https://img.shields.io/nuget/vpre/ConfigurationRepository.svg)](https://www.nuget.org/packages/ConfigurationRepository)

### Installation:

+ from [NuGet](https://www.nuget.org/packages/ConfigurationRepository);
+ from package manager console:
```
Install-Package ConfigurationRepository
```    
+ from command line:
```
dotnet add package ConfigurationRepository
```

### Usage:

The configuration can be stored in two different structured forms, we have to choose one:
+ A single configuration with it\`s keys and values, like in a dictionary, this one called `DictionaryRepository`.
+ Multiple configurations, one of which is extracted using the `Key` and a `Value` that contains parsable hierarchical structure of the configuration by that `Key`. This one called `ParsableRepository`.
> [!NOTE]
> Currently, the only format supported for `ParsableRepository` is in `JSON` format. This can be easily extended implementing `IConfigurationParser` interface for any format needed.

A dictionary repository provider is registered by calling `AddDictionaryRepository()` extension method on configuration builder.

A parsable repository provider is registered by calling `AddParsableRepository()` extension method on configuration builder.

### Examples:

Here is a simple example with two in-memory repositories:
- A dictionary repository that stores it`s data as key-value pairs.
- A parsable repository that is stored as a string parsed into a JSON.

```csharp
using ConfigurationRepository;

var builder = WebApplication.CreateBuilder(args);

// Define our sample data for dictionary repository.
var configDictData = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
{ { "DICT KEY", "DICT VALUE" } };

// Define our sample data for json repository.
var configJsonData = """{"JSON KEY":"JSON VALUE"}""";

// Add our two configuration repository providers with different
// repositories to configuration builder.
builder.Configuration
    .AddDictionaryRepository(new InMemoryDictionaryRepository(configDictData))
    .AddParsableRepository(new InMemoryJsonRepository(configJsonData));

var app = builder.Build();

app.Run();

// A dictionary in-memory repository.
class InMemoryDictionaryRepository(IDictionary<string, string?> configData)
    : IConfigurationRepository
{
    public TData GetConfiguration<TData>()
    {
        return (TData)configData;
    }
}

// A parsable json in-memory repository.
class InMemoryJsonRepository(string jsonConfig)
    : IConfigurationRepository
{
    public TData GetConfiguration<TData>()
    {
        return (TData)Convert.ChangeType(jsonConfig, typeof(TData));
    }
}
```
More complex example with database-stored configuration repository that stores secrets separatley in `Vault` and parametrizes this configuration with these secret values you may find in [ConfigurationRepository.VaultWithDbWebApp sample project on github](../../samples/ConfigurationRepository.VaultWithDbWebApp).
Note that It uses `docker` and `docker-compose` to set things up.

### See also:

The main purpose of `ConfigurationRepository` is to store the configuration in a database. The following libraries provide this:
## [Dapper configuration repository](/src/ConfigurationRepository.Dapper/README.md) - for accessing a database configuration using Dapper ORM

## [EntityFramework configuration repository](/src/ConfigurationRepository.EntityFramework/README.md) - for accessing a database configuration using Entity Frameworks ORM

## [SqlClient configuration repository](/src/ConfigurationRepository.SqlClient/README.md) - for accessing MS SQL Server database configuration using SqlClient library
