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
A simple example of using:
```csharp
using ConfigurationRepository.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddEfCoreRepository(options => options
        .UseInMemoryDatabase("ConfigurationDatabase")
        .UseTable("ConfigurationSample"))
    .Build();
```
Another example that demonstrates how we can create database context and use it to add configuration to a database and also access it through configuration service:
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
This example you may find in [ConfigurationRepository.EfCore sample project on github](../../samples/ConfigurationRepository.EfCore).
