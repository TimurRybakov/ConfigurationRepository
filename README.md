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

### See also:

The main purpose of `ConfigurationRepository` is to store the configuration in a database. The following libraries provide this:

## [Dapper configuration repository](/src/ConfigurationRepository.Dapper) - for accessing a database configuration using Dapper ORM.

```csharp
using ConfigurationRepository;
using ConfigurationRepository.Dapper;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres");

builder.Configuration.AddDapperRepository(
    repository => repository
        .UseDbConnectionFactory(() => new NpgsqlConnection(connectionString))
        .WithSelectConfigurationQuery("select \"Key\", \"Value\" from Configuration"));

var app = builder.Build();

app.Run();
```

## [EntityFramework configuration repository](/src/ConfigurationRepository.EntityFramework) - for accessing a database configuration using Entity Framework ORM.

```csharp
using ConfigurationRepository.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddEfCoreRepository(options => options
        .UseInMemoryDatabase("ConfigurationDatabase")
        .UseTable("ConfigurationTable"))
    .Build();
```

## [SqlClient configuration repository](/src/ConfigurationRepository.SqlClient) - for accessing MS SQL Server database configuration using SqlClient library.

```csharp
using ConfigurationRepository;
using ConfigurationRepository.SqlClient;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("MsSql");

builder.Configuration.AddSqlClientRepository(
    repository => repository
        .UseConnectionString(connectionString)
        .WithConfigurationTableName("Configuration"));

var app = builder.Build();

app.Run();
```

## Dapper, EntityFramework and SqlClient repositories are all reloadable. Use [Reloadable configuration repositories](/src/ReloadableConfiguration) for building configurations that periodically updates from their source providers.
```csharp
using ConfigurationRepository;
using ConfigurationRepository.Dapper;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres");

builder.Configuration.AddDapperRepository(
    repository => repository
        .UseDbConnectionFactory(() => new NpgsqlConnection(connectionString))
        .WithSelectConfigurationQuery("select \"Key\", \"Value\" from Configuration")
        .WithSelectCurrentVersionQuery("select max(\"Version\") from Configuration"),
    source => source.WithPeriodicalReload());

builder.Services.AddConfigurationReloader();

var app = builder.Build();

app.Run();
```

## [Parametrized configuration](/src/ParametrizedConfiguration) - for parametrizing configuration values
For example, this configuration described as a json:
```json
{
  "param1": "1+%param2%",
  "param2": "2+%param3%",
  "param3": "3"
}
```
will be parametrized into this:
```json
{
  "param1": "1+2+3",
  "param2": "2+3",
  "param3": "3"
}
```
```csharp
var configuration = new ConfigurationBuilder()
    // ...Here will be listed all configuration providers (at least one)...
    .WithParametrization() // Parametrized provider registered here as last one
    .Build();
```
