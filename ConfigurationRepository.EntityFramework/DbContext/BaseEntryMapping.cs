using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ConfigurationRepository.EntityFramework;

public abstract class BaseEntryMapping<TEntry> : IEntityTypeConfiguration<TEntry>
    where TEntry : class, IEntry
{
    private readonly string _tableName;
    private readonly string? _schemaName;

    protected virtual string KeyColumnName { get; } = "Key";
    protected virtual string ValueColumnName { get; } = "Value";

    protected BaseEntryMapping(string tableName, string? schemaName = null)
    {
        _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        _schemaName = schemaName;
    }

    protected virtual void ConfigureOther(EntityTypeBuilder<TEntry> builder)
    {
        //builder.HasNoKey();
    }

    public void Configure(EntityTypeBuilder<TEntry> builder)
    {
        builder.ToTable(_tableName, _schemaName);
        builder.HasKey(x => x.Key);

        builder.Property(x => x.Key).HasColumnName(KeyColumnName).IsRequired();
        builder.Property(x => x.Value).HasColumnName(ValueColumnName);

        ConfigureOther(builder);
    }
}
