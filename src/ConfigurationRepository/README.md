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
```csharp
using ConfigurationRepository;

var builder = WebApplication.CreateBuilder(args);

var configDictData = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
{ { "DICT KEY", "DICT VALUE" } };

builder.Configuration.AddDictionaryRepository(new InMemoryDictionaryRepository(configDictData));

var configJsonData = """{"JSON KEY":"JSON VALUE"}""";

builder.Configuration.AddParsableRepository(new InMemoryJsonRepository(configJsonData));

var app = builder.Build();

app.Run();

class InMemoryDictionaryRepository(IDictionary<string, string?> configData) : IConfigurationRepository
{
    public TData GetConfiguration<TData>()
    {
        return (TData)configData;
    }
}

class InMemoryJsonRepository(string jsonConfig) : IConfigurationRepository
{
    public TData GetConfiguration<TData>()
    {
        return (TData)Convert.ChangeType(jsonConfig, typeof(TData));
    }
}
```

# [Dapper configuration repository](/src/ConfigurationRepository.Dapper/README.md)

# [EntityFramework configuration repository](/src/ConfigurationRepository.EntityFramework/README.md)

# [SqlClient configuration repository](/src/ConfigurationRepository.SqlClient/README.md)
