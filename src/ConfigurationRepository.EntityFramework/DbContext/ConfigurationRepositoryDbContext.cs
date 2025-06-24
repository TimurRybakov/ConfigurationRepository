using Microsoft.EntityFrameworkCore;

namespace ConfigurationRepository.EntityFramework;

/// <summary>
/// Configuration repository database context.
/// </summary>
public class ConfigurationRepositoryDbContext(DbContextOptions<ConfigurationRepositoryDbContext> options)
    : DbContext(options), IConfigurationRepositoryDbContext<ConfigurationEntry>
{
    private readonly RepositoryDbContextOptions _options = options.FindExtension<RepositoryDbContextOptions>()
            ?? throw new InvalidOperationException($"{nameof(RepositoryDbContextOptions)} instance not found. Configure options with UseTable() extension method.");

    /// <inheritdoc/>
    public DbSet<ConfigurationEntry> ConfigurationEntryDbSet { get; set; }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ConfigurationEntryMapping(_options));

        base.OnModelCreating(modelBuilder);
    }

    private class ConfigurationEntryMapping(RepositoryDbContextOptions options)
        : BaseConfigurationEntryMapping<ConfigurationEntry>(options.TableName, options.SchemaName)
    {
        protected override string KeyColumnName { get; } = options.KeyColumnName;

        protected override string ValueColumnName { get; } = options.ValueColumnName;
    }
}
