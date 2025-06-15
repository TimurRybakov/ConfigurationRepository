using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ConfigurationRepository.EntityFramework;

/// <summary>
/// EF Core configuration entity mapping.
/// </summary>
/// <typeparam name="TEntry">Entity type.</typeparam>
public abstract class BaseConfigurationEntryMapping<TEntry>(
    string tableName,
    string? schemaName = null)
    : IEntityTypeConfiguration<TEntry> where TEntry : class, IConfigurationEntry
{
    private readonly string _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));

    /// <summary>
    /// The name for a column storing keys.
    /// </summary>
    protected virtual string KeyColumnName { get; } = "Key";

    /// <summary>
    /// The name for a column storing values.
    /// </summary>
    protected virtual string ValueColumnName { get; } = "Value";

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<TEntry> builder)
    {
        builder.ToTable(_tableName, schemaName);
        builder.HasKey(x => x.Key);

        builder.Property(x => x.Key).HasColumnName(KeyColumnName).IsRequired();
        builder.Property(x => x.Value).HasColumnName(ValueColumnName);
    }
}
