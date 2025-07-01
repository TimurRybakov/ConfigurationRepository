# ConfigurationRepository.SqlClient
An ASP .NET Core class library for using MS SQL Server databases as configuration repositories via SqlClient library.

[![NuGet](https://img.shields.io/nuget/dt/ConfigurationRepository.SqlClient.svg)](https://www.nuget.org/packages/ConfigurationRepository.SqlClient)
[![NuGet](https://img.shields.io/nuget/vpre/ConfigurationRepository.SqlClient.svg)](https://www.nuget.org/packages/ConfigurationRepository.SqlClient)

### Installation:

+ from [NuGet](https://www.nuget.org/packages/ConfigurationRepository.SqlClient);
+ from package manager console:
```
Install-Package ConfigurationRepository.SqlClient
```    
+ from command line:
```
dotnet add package ConfigurationRepository.SqlClient
```

### Usage:

The configuration can be stored in two different structured forms, we have to choose one:
+ A single configuration with it\`s keys and values, like in a dictionary.
Access to configuration storage is provided by `SqlClientDictionaryConfigurationRepository` class.
+ Multiple configurations, one of which is extracted using the `Key` and a `Value` that contains parsable hierarchical structure of the configuration by that `Key`.
Access to configuration storage is provided by `SqlClientParsableConfigurationRepository` class.

> [!NOTE]
> Currently, the only format supported for `SqlClientParsableConfigurationRepository` is in `JSON` format.
> This can be easily extended implementing `IConfigurationParser` interface for any format needed.

A dictionary repository provider using `SqlClient` is registered by calling `AddSqlClientRepository()` extension method on configuration builder.

A parsable repository provider using `SqlClient` is registered by calling `AddSqlClientJsonRepository()` extension method on configuration builder.

### Examples:

#### `SqlClientDictionaryConfigurationRepository`

Let's assume that we have a database that stores a configuration table as key-value pairs:
> ```tsql
> create table [Configuration]
> (
>     [Key]   varchar(800)  not null primary key clustered,
>     [Value] nvarchar(max)     null
> );
> 
> insert [Configuration] ([Key], [Value])
> values ('Logging:LogLevel:Default', N'Information'),
>        ('Logging:LogLevel:Microsoft.AspNetCore', N'Warning');
> ```
> This script defines a table with non-nullable Key column used as primary key and nullable Value column.
> The hierarchy of keys is linearized by colon [:] separators.
> The names of the table, columns and keys/indexes on them can be any.

So then in our application Program.cs file we may add a configuration provider like this:
> ```csharp
> using ConfigurationRepository;
> using ConfigurationRepository.SqlClient;
>
> var builder = WebApplication.CreateBuilder(args);
> 
> var connectionString = builder.Configuration.GetConnectionString("MsSql");
> 
> builder.Configuration.AddSqlClientRepository(
>     repository => repository
>         .UseConnectionString(connectionString)
>         .WithConfigurationTableName("Configuration"));
> 
> var app = builder.Build();
> 
> app.Run();
> ```
> Here we:
> - Extract connection string named `MsSql` from existing configuration providers (i.e. `appsettings.json`).
> - Register database repository configuration provider using `SqlClientDictionaryConfigurationRepository` with `AddSqlClientRepository()` extension method.
> - Define database connection that will create database connection for our provider using `UseConnectionString()` extension method passing our connection string.
> - Define the configuration table name that will be used in select configuration queries with `WithConfigurationTableName()` extension method.

#### `SqlClientDictionaryConfigurationRepository` with periodical reload

If our database source can change in time we may also add configuration reloader that will periodically reload our configuration from the database:
> ```csharp
> using ConfigurationRepository;
> using ConfigurationRepository.SqlClient;
>
> var builder = WebApplication.CreateBuilder(args);
> 
> var connectionString = builder.Configuration.GetConnectionString("MsSql");
> 
> builder.Configuration.AddSqlClientRepository(
>     repository => repository
>         .UseConnectionString(connectionString)
>         .WithConfigurationTableName("Configuration"),
>     source => source.WithPeriodicalReload());
> 
> builder.Services.AddConfigurationReloader();
> 
> var app = builder.Build();
> 
> app.Run();
> ```
> Here we additionaly:
> - Define that our configuration provider source will use `PeriodicalReload` background service.
> - Register `PeriodicalReload` background service in our service collection.
>
> We can define reload period as a time span passed as a parameter to `WithPeriodicalReload()` exstension method.

#### Versioned `SqlClientDictionaryConfigurationRepository` with periodical reload

What if our database configuration is too heavy to reload it frequently and we want to reduce our network traffic to DBMS?
We can hold version information for our configuration table adding a `Version` table:

> ```tsql
> create table [Version]
> (
>     [CurrentVersion]   rowversion   not null,
>     [PreviousVersion]  varbinary(8)     null
> );
> insert [Version] ([PreviousVersion]) values (null);
> ```
> Here we:
> + Add a `Version` table with single record containing `CurrentVersion` and `PreviousVersion` table.
> The current version is updated each change in `Configuration` table.
> Previous value of `CurrentVersion` is stored in `PreviousVersion` column.
> + Initialize `Version` table adding single row. This generates value for `CurrentVersion` column.

We also add `trigger` on configuration table that will update `CurrentVersion` and `PreviousVersion` in `Version` table:

> ```tsql
> create trigger [tr_Configuration_Change] on [Configuration] for insert, update, delete
> as
>   set nocount on;
> 
>   declare
>     @inserted_cs int = isnull((select checksum_agg([checksum]) from (select [checksum] = checksum(*) from inserted) q), 0),
>     @deleted_cs  int = isnull((select checksum_agg([checksum]) from (select [checksum] = checksum(*) from deleted) q), 0);
> 
>   if @inserted_cs != @deleted_cs
>     update [Version] set [PreviousVersion] = [CurrentVersion]; 
> ```
> Here we create `tr_Configuration_Change` trigger that will be executed after each insert, update or delete on `Configuration` table.
> The trigger then checks if inserted value really differs from existing by comparing their checksums.
> Only if it does then current verion is renewed in `Version` table by update statement.

> [!NOTE]
> A versioned repository is reloaded only if version was changed in the database.
> This reduces the network traffic sent from DBMS to application with configuration repository.

We also add `VersionTableName` registration to our repository with `WithVersionTableName()` extension method call:
> ```csharp
> using ConfigurationRepository;
> using ConfigurationRepository.SqlClient;
> 
> var builder = WebApplication.CreateBuilder(args);
> 
> var connectionString = builder.Configuration.GetConnectionString("MsSql");
> 
> builder.Configuration.AddSqlClientRepository(
>     repository => repository
>         .UseConnectionString(connectionString)
>         .WithConfigurationTableName("Configuration")
>         .WithVersionTableName("Version"),
>     source => source.WithPeriodicalReload());
> 
> builder.Services.AddConfigurationReloader();
> 
> var app = builder.Build();
> 
> app.Run();
> ```
> Now we have added `WithVersionTableName()` extension method that defines a version table name.
> A select queries to this table will be executed prior to reloading configuration.
> If version was not changed since previous execution, the configuration will not be reloaded.

#### Versioned `SqlClientParsableConfigurationRepository` with periodical reload

Let\`s see how we can deal with parsable configurations using the example of a json configuration.
First, we define configuration table:

> ```tsql
> create table [JsonConfiguration]
> (
>   [Key]       varchar(255) collate Latin1_General_100_BIN2 not null primary key clustered,
>   [JsonValue] nvarchar(max) null,
>   [Version]   rowversion not null
> );
> insert [JsonConfiguration] ([Key], [JsonValue])
> values ('MyAppConfig', N'{"ConfigurationKey1":"Value1","ConfigurationKey2":"Value2"}');
> ```
> Here we:
> + Created configuration table named `JsonConfiguration` that will store one or more records each of one is a whole configuration accessed by `Key`.
> + We created clustered primary key on `Key` column and used binary collation for faster lookups.
> + The `JsonValue` column will hold our JSON as unicode string that will be parsed into configuration on the application side.
> + The `Version` column is a `rowversion` that is automatically updated by MS SQL Server when rows are inserted or updated.
> + Inserted one row into `JsonConfiguration` table with `Key` = "MyAppConfig" and `JsonValue` with our JSON configuration.

Finally in our application we can set things up:
> ```csharp
> using ConfigurationRepository;
> using ConfigurationRepository.SqlClient;
> 
> var builder = WebApplication.CreateBuilder(args);
> 
> var connectionString = builder.Configuration.GetConnectionString("MsSql");
> 
> builder.Configuration.AddSqlClientJsonRepository(
>     repository => repository
>         .UseConnectionString(connectionString)
>         .WithConfigurationTableName("JsonConfiguration")
>         .WithValueFieldName("JsonValue")
>         .WithVersionFieldName("Version")
>         .WithKey("MyAppConfig"),
>     source => source.WithPeriodicalReload());
> 
> builder.Services.AddConfigurationReloader();
> 
> var app = builder.Build();
> 
> app.Run();
> ```
> Now we have registered a JSON configuration provider with `AddSqlClientJsonRepository()` extension method call.
> Inside it we configure our `SqlClientParsableConfigurationRepository`:
> + `UseConnectionString(connectionString)` defines that the repository will use connections with connection string from `connectionString` string variable.
> + `WithConfigurationTableName("JsonConfiguration")` defines that the repository also will use configuration table named `JsonConfiguration`.
> + `WithValueFieldName("JsonValue")` defines a name for `Value` column.
> + `WithVersionFieldName("Version")` defines a name for `Version` column.
> + `WithKey("MyAppConfig")` defines configuration `Key` value as there may be more than one configuration in the table.
>
> Also we defined that our configuration source is reloadable by specifying `source => source.WithPeriodicalReload()`.
> And registered `ConfigurationReloader` hosted service in service collection for periodical reload of configuration provider by `builder.Services.AddConfigurationReloader()`.
