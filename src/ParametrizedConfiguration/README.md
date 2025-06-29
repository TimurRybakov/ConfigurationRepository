# ParametrizedConfiguration
An ASP .NET Core class library presents a configuration provider that uses it\`s own configuration data via other providers to parametrize parameter placeholders with values, accessed by parameter keys.
By default placeholders defined between two `%` symbols like `%param name%`, where `param name` should be the key of the same configuration, the value of which will be substituted into `%param name%`.

For example, this configuration described as a json:
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
This can be used to hide sensitive data from publicly stored configurations or to reuse the same values in different parts of the configuration.

[![NuGet](https://img.shields.io/nuget/dt/ParametrizedConfiguration.svg)](https://www.nuget.org/packages/ParametrizedConfiguration)
[![NuGet](https://img.shields.io/nuget/vpre/ParametrizedConfiguration.svg)](https://www.nuget.org/packages/ParametrizedConfiguration)

### Installation:

+ from [NuGet](https://www.nuget.org/packages/ParametrizedConfiguration);
+ from package manager console:
```
Install-Package ParametrizedConfiguration
```    
+ from command line:
```
dotnet add package ParametrizedConfiguration
```

### Usage:

Parametrization will be made on any configuration that registered `ParametrizedConfigurationProvider`.
The registration is made by calling `WithParametrization()` extension method on configuration builder.

> [!NOTE]
> Call `WithParametrization()` method last before you build your configuration as it uses all providers registered before itself.

```csharp
var configuration = new ConfigurationBuilder()
    // ...Here will be listed all configuration providers (at least one)...
    .WithParametrization() // Parametrized provider registered here as last one
    .Build();
```

### Examples:

Here is a simple example to demonstrate configuration parametrization:
```csharp
using ParametrizedConfiguration;
using Microsoft.Extensions.Configuration;

// Assume secrets are set via environment variables somewere outside this code,
// we set them here just for clarity:
Environment.SetEnvironmentVariable("DatabaseName", "MyDatabase");
Environment.SetEnvironmentVariable("UserName", "Bob");
Environment.SetEnvironmentVariable("Password", "strongPassword");

// Define configuration that will be parametrized with it`s own values:
var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .WithParametrization()
    .Build();

// Let`s define our configuration key with parameters. It also won't be here
// in our production code, but will be loaded from configuration providers
// such as json-files or any other defined in ConfigurationBuilder.
configuration["ConnectionStrings:Mssql"] =
    "Server=mssql-server;Database=%DatabaseName%;User Id=%UserName%;Password=%Password%;TrustServerCertificate=True";

// Ok, now let`s get the connection string from configuration:
Console.WriteLine(configuration.GetConnectionString("mssql"));

// Output will be parametrized with values from same configuration:
// Server=mssql-server;Database=MyDatabase;User Id=Bob;Password=strongPassword;TrustServerCertificate=True
```
This example you may also find in [ParametrizedConfiguration.EnvSecrets sample project on github](../../samples/ParametrizedConfiguration.EnvSecrets).
