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
+ A single configuration with it\`s keys and values, like in a dictionary.
This type of configuration repository is called `Dictionary repository`.
+ Multiple configurations, one of which is extracted using the `Key` and a `Value` that contains parsable hierarchical structure of the configuration by that `Key`.
This type of configuration repository is called `Parsable repository`.
> [!NOTE]
> Currently, the only format supported for `Parsable repository` is in `JSON` format. This can be easily extended implementing `IConfigurationParser` interface for any format needed.

A dictionary repository provider is registered by `AddDictionaryRepository()` extension method called on configuration builder.

A parsable repository provider is registered by `AddParsableRepository()` extension method called on configuration builder.

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
More complex example with database-stored configuration repository that stores secrets separatley in `Vault` and parametrizes this configuration with these secret values you may find in [ConfigurationRepository.VaultWithDbWebApp sample project on github](https://github.com/TimurRybakov/ConfigurationRepository/tree/master/samples/ConfigurationRepository.VaultWithDbWebApp).
Note that It uses `docker` and `docker-compose` to set all environment up.

### See also:

The main purpose of `ConfigurationRepository` is to store the configuration in a database. The following libraries provide this:
## [ConfigurationRepository.Dapper](https://github.com/TimurRybakov/ConfigurationRepository/tree/master/src/ConfigurationRepository.Dapper) - for accessing database configurations using Dapper ORM.

## [ConfigurationRepository.EntityFramework](https://github.com/TimurRybakov/ConfigurationRepository/tree/master/src/ConfigurationRepository.EntityFramework) - for accessing database configurations using Entity Framework ORM.

## [ConfigurationRepository.SqlClient](https://github.com/TimurRybakov/ConfigurationRepository/tree/master/src/ConfigurationRepository.SqlClient) - for accessing MS SQL Server database configurations using MS SqlClient library.

Any of these configurations may also be parametrizable:
## [ParametrizedConfiguration](https://github.com/TimurRybakov/ConfigurationRepository/tree/master/src/ParametrizedConfiguration) - for parametrizing configuration provided by other configuration providers.
