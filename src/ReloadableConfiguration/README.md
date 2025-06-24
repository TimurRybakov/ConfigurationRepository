# ReloadableConfiguration
An ASP .NET Core class library for using databases as configuration repositories.

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

class InMemoryDictionaryRepository(IDictionary<string, string?> configData) : IRepository
{
    public TData GetConfiguration<TData>()
    {
        return (TData)configData;
    }
}

class InMemoryJsonRepository(string jsonConfig) : IRepository
{
    public TData GetConfiguration<TData>()
    {
        return (TData)Convert.ChangeType(jsonConfig, typeof(TData));
    }
}
```
