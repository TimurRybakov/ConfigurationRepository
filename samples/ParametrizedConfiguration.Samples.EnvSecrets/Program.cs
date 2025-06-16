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
