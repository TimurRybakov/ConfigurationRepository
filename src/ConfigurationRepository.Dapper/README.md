# ConfigurationRepository.Dapper
An ASP .NET Core class library for using databases as configuration repositories via Dapper ORM.

[![NuGet](https://img.shields.io/nuget/dt/ConfigurationRepository.Dapper.svg)](https://www.nuget.org/packages/ConfigurationRepository.Dapper)
[![NuGet](https://img.shields.io/nuget/vpre/ConfigurationRepository.Dapper.svg)](https://www.nuget.org/packages/ConfigurationRepository.Dapper)

### Installation:

+ from [NuGet](https://www.nuget.org/packages/ConfigurationRepository.Dapper);
+ from package manager console:
```
Install-Package ConfigurationRepository.Dapper
```    
+ from command line:
```
dotnet add package ConfigurationRepository.Dapper
```

### Usage:

The configuration can be stored in two different structured forms, we have to choose one:
+ A single configuration with it\`s keys and values, like in a dictionary, this one called `DapperDictionaryConfigurationRepository`.
+ Multiple configurations, one of which is extracted using the `Key` and a `Value` that contains parsable hierarchical structure of the configuration by that `Key`. This one called `DapperParsableConfigurationRepository`.
> [!NOTE]
> Currently, the only format supported for `DapperParsableConfigurationRepository` is in `JSON` format. This can be easily extended implementing `IConfigurationParser` interface for any format needed.

A dictionary repository provider using `Dapper` is registered by calling `AddDapperRepository()` extension method on configuration builder.

A parsable repository provider using `Dapper` is registered by calling `AddDapperJsonRepository()` extension method on configuration builder.

### Postgres examples:

> [!NOTE]
> The following examples are based on using `Postgres`, but you can use any DBMS that `Dapper` supports.

#### `DapperDictionaryConfigurationRepository`

Let's assume that we have a database that stores a configuration table as key-value pairs:
> ```tsql
> create table Configuration (
>     "Key" varchar(800) not null primary key,
>     "Value" text null
> );
> 
> insert into Configuration ("Key", "Value")
> values 
>     ('Logging:LogLevel:Default', 'Information'),
>     ('Logging:LogLevel:Microsoft.AspNetCore', 'Warning');
> ```
> This script defines a table with non-nullable Key column used as primary key and nullable Value column. The hierarchy of keys is linearized by colon [:] separators. The names of the table, columns and keys/indexes on them can be any.

So then in our application Program.cs file we may add a configuration provider like this:
> ```csharp
> using ConfigurationRepository;
> using ConfigurationRepository.Dapper;
>
> var builder = WebApplication.CreateBuilder(args);
> 
> var connectionString = builder.Configuration.GetConnectionString("Postgres");
> 
> builder.Configuration.AddDapperRepository(
>     repository => repository
>         .UseDbConnectionFactory(() => new NpgsqlConnection(connectionString))
>         .WithSelectConfigurationQuery("select \"Key\", \"Value\" from Configuration"));
> 
> var app = builder.Build();
> 
> app.Run();
> ```
> Here we:
> - Extract connection string named `Postgres` from existing configuration providers (i.e. `appsettings.json`).
> - Register database repository configuration provider using `DapperDictionaryConfigurationRepository` with `AddDapperRepository()` extension method.
> - Define database connection factory that will create database connection for our provider using `UseDbConnectionFactory()` extension method and our connection string.
> - Define the select configuration query with `WithSelectConfigurationQuery()` extension method.

#### `DapperDictionaryConfigurationRepository` with periodical reload

If our database source can change in time we may also add configuration reloader that will periodically reload our configuration from the database:
> ```csharp
> using ConfigurationRepository;
> using ConfigurationRepository.Dapper;
>
> var builder = WebApplication.CreateBuilder(args);
> 
> var connectionString = builder.Configuration.GetConnectionString("Postgres");
> 
> builder.Configuration.AddDapperRepository(
>     repository => repository
>         .UseDbConnectionFactory(() => new NpgsqlConnection(connectionString))
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
> - Define that our configuration provider source will use `PeriodicalReload` background service.
> - Register `PeriodicalReload` background service in our service collection.
>
> We can define reload period as a time span passed as a parameter to `WithPeriodicalReload()` exstension method.

#### Versioned `DapperDictionaryConfigurationRepository` with periodical reload

What if our database configuration is too heavy to reload it frequently and we want to reduce our network traffic to DBMS?
We can version our configurations adding a `rowversion` column to the configuration table:
> ```tsql
> create table Configuration (
>     "Key" varchar(800) not null primary key,
>     "Value" text null,
>     "Version" bytea not null unique
> );
> ```
> Here we additionaly:
> - Add a `Version` column of type `bytea` to `Configuration` table.
> - Mark `Version` column with `uniqe constraint` to get an indexed column.

We also should define automatic increment for our `Version` column for inserts and updates.
This can be done with a trigger with a function:
```tsql
create or replace function swf_increment_configuration_version()
  returns trigger
as
$$
begin
  if exists(select 1 from Configuration) then
    new."Version" := int8send((select max ('x'||lpad(encode("Version", 'hex'), 16, '0'))::bit(64)::bigint from Configuration)+1::bigint);
    return new;
  else
    new."Version" := int8send(1);
    return new;
  end if;
end;
$$
language plpgsql;
```
And a trigger itself:
```tsql
create trigger swt_increment_configuration_version
  before insert or update on Configuration
  for each row
    execute procedure swf_increment_configuration_version();
```
Then we can initialize our `Configuration` table:
```tsql
insert into Configuration ("Key", "Value")
values 
    ('Logging:LogLevel:Default', 'Information'),
    ('Logging:LogLevel:Microsoft.AspNetCore', 'Warning');
```

> [!NOTE]
> A versioned repository is reloaded only if version was changed in the database.
> This reduces the network traffic sent from DBMS to application with configuration repository.

On application side we add `SelectCurrentVersionQuery` registration to our repository with `WithSelectCurrentVersionQuery()` extension method call:
> ```csharp
> using ConfigurationRepository;
> using ConfigurationRepository.Dapper;
> 
> var builder = WebApplication.CreateBuilder(args);
> 
> var connectionString = builder.Configuration.GetConnectionString("Postgres");
> 
> builder.Configuration.AddDapperRepository(
>     repository => repository
>         .UseDbConnectionFactory(() => new NpgsqlConnection(connectionString))
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
> Now we have added `WithSelectCurrentVersionQuery()` extension method that defines a query that will select the current configuration version.
> It will be used before downloading the configuration from the repository. If version was not changed since previous execution, the configuration will not be reloaded.

#### Versioned `DapperParsableConfigurationRepository` with periodical reload

Let\`s see how we can deal with parsable configurations using the example of a json configuration.
First, we define configuration table:

> ```tsql
> create table JsonConfiguration
> (
>   "Key" varchar(255) not null primary key,
>   "JsonValue" text null,
>   "Version" bytea not null unique
> );
> insert JsonConfiguration ("Key", "JsonValue")
> values ('MyAppConfig', '{"ConfigurationKey1":"Value1","ConfigurationKey2":"Value2"}');
> ```
> Here we:
> + Created configuration table named `JsonConfiguration` that will store one or more records each of one is a whole configuration accessed by `Key`.
> + We created clustered primary key on `Key` column and used binary collation for faster lookups.
> + The `JsonValue` column will hold our JSON as unicode string that will be parsed into configuration on the application side.
> + The `Version` column is a `bytea` that is automatically updated by a trigger when rows are inserted, updated or deleted.
> + Inserted one row into `JsonConfiguration` table with `Key` = "MyAppConfig" and `JsonValue` with our JSON configuration.

We also should define automatic increment for our `Version` column for inserts and updates.
This can be done with a trigger with a function:
```tsql
create or replace function swf_increment_json_configuration_version()
  returns trigger
as
$$
begin
  if exists(select 1 from JsonConfiguration) then
    new."Version" := int8send((select max ('x'||lpad(encode("Version", 'hex'), 16, '0'))::bit(64)::bigint from JsonConfiguration)+1::bigint);
    return new;
  else
    new."Version" := int8send(1);
    return new;
  end if;
end;
$$
language plpgsql;
```
And a trigger itself:
```tsql
create trigger swt_increment_json_configuration_version
  before insert or update on JsonConfiguration
  for each row
    execute procedure swf_increment_json_configuration_version();
```
Then we can initialize our `JsonConfiguration` table:
```tsql
insert into Configuration ("Key", "JsonValue")
values ('MyAppConfig', '{"ConfigurationKey1":"Value1","ConfigurationKey2":"Value2"}');
```

Finally in our application we can set things up:
> ```csharp
> using ConfigurationRepository;
> using ConfigurationRepository.Dapper;
> 
> var builder = WebApplication.CreateBuilder(args);
> 
> var connectionString = builder.Configuration.GetConnectionString("Postgres");
> 
> builder.Configuration.AddDapperJsonRepository(
>     repository => repository
>         .UseDbConnectionFactory(() => new NpgsqlConnection(connectionString))
>         .WithSelectConfigurationQuery("select \"JsonValue\" as \"Value\" from JsonConfiguration where \"Key\" = @Key")
>         .WithSelectCurrentVersionQuery("select \"Version\" from JsonConfiguration where \"Key\" = @Key"),
>         .WithKey("MyAppConfig"),
>     source => source.WithPeriodicalReload());
> 
> builder.Services.AddConfigurationReloader();
> 
> var app = builder.Build();
> 
> app.Run();
> ```
> Now we have registered a JSON configuration provider with `AddDapperJsonRepository()` extension method call.
> Inside it we configure our `DapperParsableConfigurationRepository`:
> + `UseDbConnectionFactory(() => new NpgsqlConnection(connectionString))` defines that the repository will use connections provided by `() => new NpgsqlConnection(connectionString)` factory method.
> + `WithSelectConfigurationQuery()` defines the select configuration query for the repository.
> + `WithSelectCurrentVersionQuery()` defines the select configuration version query for the repository.
> + `WithKey("MyAppConfig")` defines configuration `Key` value as there may be more than one configuration in the table.
>
> Also we defined that our configuration source is reloadable by specifying `source => source.WithPeriodicalReload()`.
> And registered `ConfigurationReloader` hosted service in service collection for periodical reload of configuration provider by `builder.Services.AddConfigurationReloader()`.

### MS SQL Server examples:

> [!NOTE]
> The following examples are based on using `MS SQL Server`, but you can use any DBMS that `Dapper` supports.

#### `DapperDictionaryConfigurationRepository`

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
> using ConfigurationRepository;
> using ConfigurationRepository.Dapper;
>
> var builder = WebApplication.CreateBuilder(args);
> 
> var connectionString = builder.Configuration.GetConnectionString("MsSql");
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
> - Extract connection string named `MsSql` from existing configuration providers (i.e. `appsettings.json`).
> - Register database repository configuration provider using `DapperDictionaryConfigurationRepository` with `AddDapperRepository()` extension method.
> - Define database connection factory that will create database connection for our provider using `UseDbConnectionFactory()` extension method and our connection string.
> - Define the select configuration query with `WithSelectConfigurationQuery()` extension method.

#### `DapperDictionaryConfigurationRepository` with periodical reload

If our database source can change in time we may also add configuration reloader that will periodically reload our configuration from the database:
> ```csharp
> using ConfigurationRepository;
> using ConfigurationRepository.Dapper;
>
> var builder = WebApplication.CreateBuilder(args);
> 
> var connectionString = builder.Configuration.GetConnectionString("MsSql");
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
> - Define that our configuration provider source will use `PeriodicalReload` background service.
> - Register `PeriodicalReload` background service in our service collection.
>
> We can define reload period as a time span passed as a parameter to `WithPeriodicalReload()` exstension method.

#### Versioned `DapperDictionaryConfigurationRepository` with periodical reload

What if our database configuration is too heavy to reload it frequently and we want to reduce our network traffic to DBMS?
We can version our configurations adding a `rowversion` column to the configuration table:
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
> - Add a `Version` column of type `rowversion` to `Configuration` table.
> - Mark `Version` column with `uniqe constraint` to get an indexed column.

> [!NOTE]
> A versioned repository is reloaded only if version was changed in the database.
> This reduces the network traffic sent from DBMS to application with configuration repository.

We also add `SelectCurrentVersionQuery` registration to our repository with `WithSelectCurrentVersionQuery()` extension method call:
> ```csharp
> using ConfigurationRepository;
> using ConfigurationRepository.Dapper;
> 
> var builder = WebApplication.CreateBuilder(args);
> 
> var connectionString = builder.Configuration.GetConnectionString("MsSql");
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
> Now we have added `WithSelectCurrentVersionQuery()` extension method that defines a query that will select the current configuration version.
> It will be used before downloading the configuration from the repository. If version was not changed since previous execution, the configuration will not be reloaded.

#### Versioned `DapperParsableConfigurationRepository` with periodical reload

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
> using ConfigurationRepository.Dapper;
> 
> var builder = WebApplication.CreateBuilder(args);
> 
> var connectionString = builder.Configuration.GetConnectionString("MsSql");
> 
> builder.Configuration.AddDapperJsonRepository(
>     repository => repository
>         .UseDbConnectionFactory(() => new SqlConnection(connectionString))
>         .WithSelectConfigurationQuery("select JsonValue as \"Value\" from [JsonConfiguration] where \"Key\" = @Key")
>         .WithSelectCurrentVersionQuery("select Version from [JsonConfiguration] where \"Key\" = @Key"),
>         .WithKey("MyAppConfig"),
>     source => source.WithPeriodicalReload());
> 
> builder.Services.AddConfigurationReloader();
> 
> var app = builder.Build();
> 
> app.Run();
> ```
> Now we have registered a JSON configuration provider with `AddDapperJsonRepository()` extension method call.
> Inside it we configure our `DapperParsableConfigurationRepository`:
> + `UseDbConnectionFactory(() => new SqlConnection(connectionString))` defines that the repository will use connections provided by `() => new SqlConnection(connectionString)` factory method.
> + `WithSelectConfigurationQuery()` defines the select configuration query for the repository.
> + `WithSelectCurrentVersionQuery()` defines the select configuration version query for the repository.
> + `WithKey("MyAppConfig")` defines configuration `Key` value as there may be more than one configuration in the table.
>
> Also we defined that our configuration source is reloadable by specifying `source => source.WithPeriodicalReload()`.
> And registered `ConfigurationReloader` hosted service in service collection for periodical reload of configuration provider by `builder.Services.AddConfigurationReloader()`.
