using ConfigurationRepository.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace ConfigurationRepository.EntityFramework;

/// <summary>
/// A factory method that creates configured database context options.
/// </summary>
public static class ConfigurationDbContextOptionsFactory
{
    /// <summary>
    /// Creates an instance of <see cref="DbContextOptions{ConfigurationRepositoryDbContext}"/>,
    /// configures it in external call and returns builded <see cref="DbContextOptions{ConfigurationRepositoryDbContext}"/>.
    /// </summary>
    /// <param name="configureOptions">An options configurator method.</param>
    /// <returns>Created database context options.</returns>
    public static DbContextOptions<ConfigurationRepositoryDbContext> Create(
        Action<DbContextOptionsBuilder<ConfigurationRepositoryDbContext>> configureOptions)
    {
        var options = new DbContextOptionsBuilder<ConfigurationRepositoryDbContext>();
        configureOptions(options);

        // If RepositoryDbContextOptions was not configured, create default one.
        if (options.Options.FindExtension<RepositoryDbContextOptions>() is null)
        {
            options.UseTable("Configuration");
        }
        return options.Options;
    }
}
