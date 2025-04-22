using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ConfigurationRepository.EntityFramework;

public abstract class BaseDictionaryConfigurationEntryMapping<TEntry> : IEntityTypeConfiguration<TEntry>
    where TEntry : class, IDictionaryConfigurationEntry
{
    private readonly string _tableName;
    private readonly string? _schemaName;

    protected virtual string KeyColumnName { get; } = "Key";

    protected virtual string ValueColumnName { get; } = "Value";

    protected BaseDictionaryConfigurationEntryMapping(string tableName, string? schemaName = null)
    {
        _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        _schemaName = schemaName;
    }

    public void Configure(EntityTypeBuilder<TEntry> builder)
    {
        builder.ToTable(_tableName, _schemaName);
        builder.HasKey(x => x.Key);

        builder.Property(x => x.Key).HasColumnName(KeyColumnName).IsRequired();
        builder.Property(x => x.Value).HasColumnName(ValueColumnName);
    }
}
