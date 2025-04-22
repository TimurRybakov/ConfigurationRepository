using Microsoft.EntityFrameworkCore;

namespace ConfigurationRepository.EntityFramework;

public class DictionaryRepositoryDbContext
    : DbContext, IDictionaryRepositoryDbContext<DictionaryConfigurationEntry>
{
    private readonly DictionaryRepositoryDbContextOptions _options;

    public DbSet<DictionaryConfigurationEntry> ConfigurationEntryDbSet { get; set; }

    public DictionaryRepositoryDbContext(DbContextOptions<DictionaryRepositoryDbContext> options)
        : base(options)
    {
        _options = options.FindExtension<DictionaryRepositoryDbContextOptions>()
            ?? throw new InvalidOperationException($"{nameof(DictionaryRepositoryDbContextOptions)} instance not found. Configure options with UseTable() extension method.");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ConfigurationEntryMapping(_options));

        base.OnModelCreating(modelBuilder);
    }

    private class ConfigurationEntryMapping : BaseDictionaryConfigurationEntryMapping<DictionaryConfigurationEntry>
    {
        protected override string KeyColumnName { get; }

        protected override string ValueColumnName { get; }

        public ConfigurationEntryMapping(DictionaryRepositoryDbContextOptions options)
            : base(options.TableName, options.SchemaName)
        {
            KeyColumnName = options.KeyColumnName;
            ValueColumnName = options.ValueColumnName;
        }
    }
}
