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

# [Dapper configuration repository](/src/ConfigurationRepository.Dapper/README.md)

# [EntityFramework configuration repository](/src/ConfigurationRepository.EntityFramework/README.md)

# [SqlClient configuration repository](/src/ConfigurationRepository.SqlClient/README.md)
# Ms Sql Server + Dapper:

Let's assume that we have a database that stores a configuration table as key-value pairs:
> ```tsql
> create table Configuration
> (
>     [Key]   varchar(800)  not null primary key clustered,
>     [Value] nvarchar(max)     null
> );
> 
> insert Configuration ([Key], [Value])
> values ('Logging:LogLevel:Default', N'Information'),
>        ('Logging:LogLevel:Microsoft.AspNetCore', N'Warning');
> ```
> This script defines a table with non-nullable Key column used as primary key and nullable Value column. The hierarchy of keys is linearized by colon [:] separators. The names of the table, columns and keys/indexes on them can be any.

So then in our application Program.cs file we may add a configuration provider like this:
> ```csharp
> var builder = WebApplication.CreateBuilder(args);
> 
> var connectionString = builder.Configuration.GetConnectionString("Dapper");
> 
> builder.Configuration.AddDapperRepository(
>     repository => repository
>         .UseDbConnectionFactory(() => new SqlConnection(connectionString))
>         .WithSelectConfigurationQuery("select \"Key\", \"Value\" from Configuration"));
> 
> var app = builder.Build();
> 
> app.Run();
> ```
> Here we:
> - extract connection string named "Dapper" from existing configuration providers (i.e. `appsettings.json`);
> - add database repository using Dapper with `AddDapperRepository()` extension method;
> - define database connection factory that will create database connection for our provider using `UseDbConnectionFactory()` extension method and our connection string;
> - define the select configuration query with `WithSelectConfigurationQuery()` extension method.

If our database source can change at any time in any way we may also add configuration reloader that with periodically reload our configuration from database:
> ```csharp
> var builder = WebApplication.CreateBuilder(args);
> 
> var connectionString = builder.Configuration.GetConnectionString("Dapper");
> 
> builder.Configuration.AddDapperRepository(
>     repository => repository
>         .UseDbConnectionFactory(() => new SqlConnection(connectionString))
>         .WithSelectConfigurationQuery("select \"Key\", \"Value\" from Configuration"),
>     source => source.WithPeriodicalReload());
> 
> builder.Services.AddConfigurationReloader();
> 
> var app = builder.Build();
> 
> app.Run();
> ```
> Here we additionaly:
> - define that our configuration provider source will use `PeriodicalReload` background service;
> - register `PeriodicalReload` background service in our service collection.
>
> We can define reload period as a time span passed as a parameter to `WithPeriodicalReload()` exstension method.

What if our config in database is too heavy to reload it frequently and we want to minimize our network traffic? Let`s just version our configurations adding a rowversion column to the configuration table:
> ```tsql
> create table Configuration
> (
>     [Key]     varchar(800)  not null primary key clustered,
>     [Value]   nvarchar(max)     null,
>     [Version] rowversion    not null unique
> );
> 
> insert Configuration ([Key], [Value])
> values ('Logging:LogLevel:Default', N'Information'),
>        ('Logging:LogLevel:Microsoft.AspNetCore', N'Warning');
> ```
> Here we additionaly:
> - add a Version column of type rowversion to Configuration table;
> - mark Version column with uniqe constraint to get an indexed column.

Then we make our configuration versioned by adding SelectCurrentVersionQuery to our repository:
> ```csharp
> var builder = WebApplication.CreateBuilder(args);
> 
> var connectionString = builder.Configuration.GetConnectionString("Dapper");
> 
> builder.Configuration.AddDapperRepository(
>     repository => repository
>         .UseDbConnectionFactory(() => new SqlConnection(connectionString))
>         .WithSelectConfigurationQuery("select \"Key\", \"Value\" from Configuration")
>         .WithSelectCurrentVersionQuery("select max(\"Version\") from Configuration"),
>     source => source.WithPeriodicalReload());
> 
> builder.Services.AddConfigurationReloader();
> 
> var app = builder.Build();
> 
> app.Run();
> ```
> Here we additionaly add `WithSelectCurrentVersionQuery()` extension method passing query that selects current configuration version.

# ParametrizedConfiguration
ParametrizedConfiguration library presents a configuration provider that uses it\`s own configuration data via other providers to parametrize parameter placeholders with values, accessed by parameter keys. By default placeholders defined between two `%` symbols like `%param name%`, where `param name` should be the key of the same configuration, the value of which will be substituted into `%param name%`. for example:
This configuration:
```
{
  { "param1", "1+%param2%" },
  { "param2", "2+%param3%" },
  { "param3", "3" }
};
```
will be parametrized into this:
```
{
  { "param1", "1+2+3" },
  { "param2", "2+3" },
  { "param3", "3" }
};
```
This can be used to hide sensitive data from publicly stored configurations or to reuse same configuration values in several places. The code below demonstrates this:
> ```csharp
> using ParametrizedConfiguration;
> using Microsoft.Extensions.Configuration;
> 
> // Assume secrets are set via environment variables somewere outside this code,
> // we set them here just for clarity:
> Environment.SetEnvironmentVariable("DatabaseName", "MyDatabase");
> Environment.SetEnvironmentVariable("UserName", "Bob");
> Environment.SetEnvironmentVariable("Password", "strongPassword");
> 
> // Define configuration that will be parametrized with it`s own values:
> var configuration = new ConfigurationBuilder()
>     .AddEnvironmentVariables()
>     .WithParametrization()
>     .Build();
> 
> // Let`s define our configuration key with parameters. It also won't be here
> // in our production code, but will be loaded from configuration providers
> // such as json-files or any other defined in ConfigurationBuilder.
> configuration["ConnectionStrings:Mssql"] =
>     "Server=mssql-server;Database=%DatabaseName%;User Id=%UserName%;Password=%Password%;TrustServerCertificate=True";
> 
> // Ok, now let`s get the connection string from configuration:
> Console.WriteLine(configuration.GetConnectionString("mssql"));
> 
> // Output will be parametrized with values from same configuration:
> // Server=mssql-server;Database=MyDatabase;User Id=Bob;Password=strongPassword;TrustServerCertificate=True
> ```
