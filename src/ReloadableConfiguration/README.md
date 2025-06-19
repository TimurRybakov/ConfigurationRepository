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
> using ConfigurationRepository;
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
