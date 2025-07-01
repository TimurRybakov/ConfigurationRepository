# ConfigurationRepository.EntityFramework
An ASP .NET Core class library for using databases as configuration repositories via Entity Framework ORM.

[![NuGet](https://img.shields.io/nuget/dt/ConfigurationRepository.EntityFramework.svg)](https://www.nuget.org/packages/ConfigurationRepository.EntityFramework)
[![NuGet](https://img.shields.io/nuget/vpre/ConfigurationRepository.EntityFramework.svg)](https://www.nuget.org/packages/ConfigurationRepository.EntityFramework)

### Installation:

+ from [NuGet](https://www.nuget.org/packages/ConfigurationRepository.EntityFramework);
+ from package manager console:
```
Install-Package ConfigurationRepository.EntityFramework
```    
+ from command line:
```
dotnet add package ConfigurationRepository.EntityFramework
```

### Usage:

The configuration can be stored in two different structured forms, we have to choose one:
+ A table of single configuration with it\`s keys and values, like in a dictionary.
Access to configuration storage is provided by `EfDictionaryConfigurationRepository` class.
+ A table of multiple configurations, one of which is extracted using the `Key` and a `Value` that contains parsable hierarchical structure of the configuration by that `Key`.
Access to configuration storage is provided by `EfParsableConfigurationRepository` class.

> [!NOTE]
> Currently, the only format supported for `EfParsableConfigurationRepository` is in `JSON` format. This can be easily extended implementing `IConfigurationParser` interface for any format needed.

Registration of EfCore configuration provider with `EfDictionaryConfigurationRepository` for `ConfigurationBuilder`:
> ```csharp
> using ConfigurationRepository.EntityFramework;
> using Microsoft.EntityFrameworkCore;
> using Microsoft.Extensions.Configuration;
> 
> var config = new ConfigurationBuilder()
>     .AddEfCoreRepository(options => options
>         .UseInMemoryDatabase("ConfigurationDatabase")
>         .UseTable("ConfigurationTable"))
>     .Build();
> ```
> Here we:
> + Create configuration builder `ConfigurationBuilder`;
> + Register EfCore dictionary repository `EfDictionaryConfigurationRepository` for this builder as configuration provider using `AddEfCoreRepository()` extension method;
> + Configure database context options:
>   + For simplicity we are using in-memory database with `UseInMemoryDatabase()` extension method. This can be replaced by any database supported by Entity Framework.
>   + With `UseTable()` extension method we set the name for our configuration table.
> + Build configuration.

Registration of EfCore configuration provider with `EfParsableConfigurationRepository` (JSON) for `ConfigurationBuilder`:
> ```csharp
> using ConfigurationRepository.EntityFramework;
> using Microsoft.EntityFrameworkCore;
> using Microsoft.Extensions.Configuration;
> 
> var config = new ConfigurationBuilder()
>     .AddEfCoreJsonRepository("ConfigurationKey", options => options
>         .UseInMemoryDatabase("ConfigurationDatabase")
>         .UseTable("ConfigurationTable"))
>     .Build();
> ```
> Here we:
> + Create configuration builder `ConfigurationBuilder`;
> + Register EfCore parsable repository `EfParsableConfigurationRepository` for this builder as configuration provider with `JsonConfigurationParser` using `AddEfCoreJsonRepository()` extension method and set the `Key` to concrete configuration;
> + Configure database context options:
>   + For simplicity we are using in-memory database with `UseInMemoryDatabase()` extension method. This can be replaced by any database supported by Entity Framework.
>   + Using `UseTable()` extension method we set the name for our configuration table.
> + Build configuration.

### Examples:

An example that demonstrates how we can create database context and use it to add configuration to a database and also access it through configuration service.
This one is base on `EfDictionaryConfigurationRepository`:
```csharp
using ConfigurationRepository;
using ConfigurationRepository.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

// Create configuration databae context using in-memory database for simplicity.
var options = ConfigurationDbContextOptionsFactory.Create(
    options => options.UseInMemoryDatabase("TestDb"));

// Create configuration context using these options.
await using var configurationContext = new ConfigurationRepositoryDbContext(options);

// Add a key-value pair to configuration table.
configurationContext.ConfigurationEntryDbSet.Add(new ConfigurationEntry("Key", "value"));
await configurationContext.SaveChangesAsync();

// Create configuration using Entity Framework repository with created database context options.
var config = new ConfigurationBuilder()
    .AddEfCoreRepository(options)
    .Build();

// Get a value from the configuration.
Console.WriteLine(config["key"]);
```
And in case we intend to use `EfParsableConfigurationRepository`:
```csharp
using ConfigurationRepository;
using ConfigurationRepository.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

// Create configuration databae context using in-memory database for simplicity.
var options = ConfigurationDbContextOptionsFactory.Create(
    options => options.UseInMemoryDatabase("TestDb"));

// Create configuration context using these options.
await using var configurationContext = new ConfigurationRepositoryDbContext(options);

// Add a configuration to configurations table with the Key = "DatabaseConfig".
configurationContext.ConfigurationEntryDbSet.Add(new ConfigurationEntry("DatabaseConfig", """
    {
      "ConnectionStrings": {
        "Host": "A connection string to a host"
      }
    }
    """));
await configurationContext.SaveChangesAsync();

// Create configuration using Entity Framework json repository with created database context options.
var config = new ConfigurationBuilder()
    .AddEfCoreJsonRepository("DatabaseConfig", options)
    .Build();

// Get the Value = "A connection string to a host" from the configuration.
Console.WriteLine(config.GetConnectionString("Host"));
```
This example you may find in [ConfigurationRepository.EfCore sample project on github](../../samples/ConfigurationRepository.EfCore).

If our database source can change in time we may also add configuration reloader that will periodically reload our configuration from database:
```csharp
using ConfigurationRepository;
using ConfigurationRepository.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEfCoreRepository(
    options => options
        .UseInMemoryDatabase("ConfigurationDatabase")
        .UseTable("ConfigurationTable"),
    source => source.WithPeriodicalReload());

builder.Services.AddConfigurationReloader();

var app = builder.Build();

app.Run();
```
