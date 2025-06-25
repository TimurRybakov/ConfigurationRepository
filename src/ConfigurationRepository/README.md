# ConfigurationRepository
An ASP .NET Core class library for using databases as configuration repositories.

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

The main purpose of `ConfigurationRepository` is to store the configuration in a database. The following libraries provide this:
# [Dapper configuration repository](/src/ConfigurationRepository.Dapper/README.md)

# [EntityFramework configuration repository](/src/ConfigurationRepository.EntityFramework/README.md)

# [SqlClient configuration repository](/src/ConfigurationRepository.SqlClient/README.md)
