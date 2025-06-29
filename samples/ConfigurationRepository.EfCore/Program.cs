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

