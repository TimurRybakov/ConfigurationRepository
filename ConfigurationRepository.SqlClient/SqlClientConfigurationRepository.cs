using System.Collections;
using System;
using System.Diagnostics.CodeAnalysis;

namespace ConfigurationRepository.SqlClient;

public abstract class SqlClientConfigurationRepository : IVersionedRepository
{
    protected byte[]? _version;

    /// <summary>
    /// The connection string to connect to the database with configuration.
    /// </summary>
    [DisallowNull]
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Table name with configuration.
    /// </summary>
    [DisallowNull]
    public string? ConfigurationTableName { get; set; }

    /// <summary>
    /// Key field name that contains <see cref="Key"/> values.
    /// </summary>
    [DisallowNull]
    public string KeyFieldName { get; set; } = "[Key]";

    /// <summary>
    /// Field name in <see cref="ConfigurationTableName"/> table with configuration.
    /// </summary>
    [DisallowNull]
    public string ValueFieldName { get; set; } = "[Value]";

    public abstract TData GetConfiguration<TData>();

    public bool VersionChanged()
    {
        if (!IsVersioned())
            return true;

        var newVersion = GetCurrentVersion();

        if (StructuralComparisons.StructuralComparer.Compare(newVersion, _version) == 0)
        {
            return false;
        }

        _version = newVersion;
        return true;
    }

    protected abstract byte[]? GetCurrentVersion();

    protected abstract bool IsVersioned();

    protected void CheckVersionInitialized()
    {
        if (_version is null && IsVersioned())
        {
            _version = GetCurrentVersion();
        }
    }

    protected virtual void ThrowIfPropertiesNotSet()
    {
        _ = ConnectionString ?? throw new ArgumentNullException(nameof(ConnectionString));
        _ = ConfigurationTableName ?? throw new ArgumentNullException(nameof(ConfigurationTableName));
        _ = KeyFieldName ?? throw new ArgumentNullException(nameof(KeyFieldName));
        _ = ValueFieldName ?? throw new ArgumentNullException(nameof(ValueFieldName));
    }
}
