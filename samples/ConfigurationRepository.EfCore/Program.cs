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

