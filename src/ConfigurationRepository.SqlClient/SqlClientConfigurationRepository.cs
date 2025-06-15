using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository.SqlClient;

/// <summary>
/// A configuration storage repository in a database accessed using SqlClient library.
/// </summary>
public abstract class SqlClientConfigurationRepository : VersionedRepository
{
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
    /// Key field name in <see cref="ConfigurationTableName"/> configuration table.
    /// Configuration keys delimited by <see cref="ConfigurationPath.KeyDelimiter"/>. Column is not null, case ingnored.
    /// </summary>
    [DisallowNull]
    public string KeyFieldName { get; set; } = "[Key]";

    /// <summary>
    /// Value field name in <see cref="ConfigurationTableName"/> configuration table. Column is nullable.
    /// </summary>
    [DisallowNull]
    public string ValueFieldName { get; set; } = "[Value]";

    /// <inheritdoc/>
    protected virtual void ThrowIfPropertiesNotSet()
    {
        _ = ConnectionString ?? throw new ArgumentNullException(nameof(ConnectionString));
        _ = ConfigurationTableName ?? throw new ArgumentNullException(nameof(ConfigurationTableName));
        _ = KeyFieldName ?? throw new ArgumentNullException(nameof(KeyFieldName));
        _ = ValueFieldName ?? throw new ArgumentNullException(nameof(ValueFieldName));
    }
}
