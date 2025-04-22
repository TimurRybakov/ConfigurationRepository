using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.SqlClient;

namespace ConfigurationRepository.SqlClient;

public sealed class SqlClientJsonConfigurationRepository : SqlClientConfigurationRepository, IVersionedRepository
{
    private byte[]? _version;

    /// <summary>
    /// Table name with configuration.
    /// </summary>
    [DisallowNull]
    public string? ConfigurationTableName { get; set; }

    /// <summary>
    /// Version field name. If set, repository will check version change before reloading.
    /// </summary>
    public string? VersionFieldName { get; set; }

    /// <summary>
    /// Key field name that contains <see cref="Key"/> values.
    /// </summary>
    [DisallowNull]
    public string KeyFieldName { get; set; } = "[Key]";

    /// <summary>
    /// Field name in <see cref="ConfigurationTableName"/> table with configuration in json format.
    /// </summary>
    [DisallowNull]
    public string ValueFieldName { get; set; } = "[JsonValue]";

    /// <summary>
    /// The key value of <see cref="KeyFieldName"/> in <see cref="ConfigurationTableName"/> table that identifies desired configuration record.
    /// </summary>
    [DisallowNull]
    public string? Key { get; set; }

    public IRetrievalStrategy RetrievalStrategy { get; }

    public SqlClientJsonConfigurationRepository()
    {
        RetrievalStrategy = new ParsableRetrievalStrategy(() => new JsonConfigurationParser());
    }

    public TData GetConfiguration<TData>()
    {
        return (TData)Convert.ChangeType(GetConfiguration(), typeof(TData));
    }

    public bool VersionChanged()
    {
        if (VersionFieldName is null)
            return true;

        var newVersion = GetCurrentVersion();

        if (StructuralComparisons.StructuralComparer.Compare(newVersion, _version) == 0)
        {
            return false;
        }

        _version = newVersion;
        return true;
    }

    private string GetConfiguration()
    {
        ThrowIfPropertiesNotSet();
        CheckVersionInitialized();

        string selectConfigurationQuery = $"""
            select {ValueFieldName}
            from {ConfigurationTableName}
            where {KeyFieldName} = @Key
            """;

        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(selectConfigurationQuery, connection);
        command.Parameters.AddWithValue("@Key", Key);

        command.Connection.Open();

        using var reader = command.ExecuteReader();

        return !reader.Read() || reader.IsDBNull(0)
            ? throw new InvalidOperationException("Null configuration returned from server.")
            : reader.GetString(0);
    }

    private void CheckVersionInitialized()
    {
        if (_version is null && VersionFieldName is not null)
        {
            _version = GetCurrentVersion();
        }
    }

    private byte[]? GetCurrentVersion()
    {
        string selectCurrentVersionQuery = $"""
            select {VersionFieldName}
            from {ConfigurationTableName}
            where {KeyFieldName} = @Key
            """;

        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(selectCurrentVersionQuery, connection);
        command.Parameters.AddWithValue("@Key", Key);

        command.Connection.Open();

        return (byte[]?)command.ExecuteScalar();
    }

    private void ThrowIfPropertiesNotSet()
    {
        _ = ConnectionString ?? throw new ArgumentNullException(nameof(ConnectionString));
        _ = ConfigurationTableName ?? throw new ArgumentNullException(nameof(ConfigurationTableName));
        _ = Key ?? throw new ArgumentNullException(nameof(Key));
        _ = KeyFieldName ?? throw new ArgumentNullException(nameof(KeyFieldName));
        _ = ValueFieldName ?? throw new ArgumentNullException(nameof(ValueFieldName));
    }
}
