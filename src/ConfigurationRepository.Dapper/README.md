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
> using ConfigurationRepository;
>
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
> using ConfigurationRepository;
>
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
> using ConfigurationRepository;
> 
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
