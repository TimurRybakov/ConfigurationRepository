using Microsoft.EntityFrameworkCore;

namespace ConfigurationRepository.EntityFramework;

public sealed partial class RepositoryDbContext
    : DbContext, IRepositoryDbContext<ConfigurationEntry>
{
    private readonly RepositoryDbContextOptions _options;

    public DbSet<ConfigurationEntry> ConfigurationEntryDbSet { get; set; }

    public RepositoryDbContext(DbContextOptions<RepositoryDbContext> options)
        : base(options)
    {
        _options = options.FindExtension<RepositoryDbContextOptions>()
            ?? throw new InvalidOperationException($"{nameof(RepositoryDbContextOptions)} instance not found. Configure options with UseTable() extension method.");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ConfigurationEntryMapping(_options));

        base.OnModelCreating(modelBuilder);
    }

    private class ConfigurationEntryMapping : BaseEntryMapping<ConfigurationEntry>
    {
        protected override string KeyColumnName { get; }
        protected override string ValueColumnName { get; }

        public ConfigurationEntryMapping(RepositoryDbContextOptions options)
            : base(options.TableName, options.SchemaName)
        {
            KeyColumnName = options.KeyColumnName;
            ValueColumnName = options.ValueColumnName;
        }
    }
}
