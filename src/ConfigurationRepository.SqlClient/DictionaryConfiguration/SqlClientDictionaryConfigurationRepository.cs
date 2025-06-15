using Microsoft.Data.SqlClient;

namespace ConfigurationRepository.SqlClient;

/// <summary>
/// A versioned dictionary repository that uses SqlClient to fetch data from database.
/// Configuration is stored in key-value pairs.
/// </summary>
public sealed class SqlClientDictionaryConfigurationRepository : SqlClientConfigurationRepository
{
    /// <summary>
    /// A table containing current configuration version. If set, repository will check version change before reloading.
    /// </summary>
    public string? VersionTableName { get; set; }

    /// <summary>
    /// Version field name.
    /// </summary>
    public string? VersionFieldName { get; set; } = "[CurrentVersion]";

    /// <inheritdoc/>
    public override TData GetConfiguration<TData>() => (TData)(IDictionary<string, string?>)GetConfiguration();

    private Dictionary<string, string?> GetConfiguration()
    {
        ThrowIfPropertiesNotSet();
        CheckVersionInitialized();

        string selectConfigurationQuery = $"select {KeyFieldName}, {ValueFieldName} from {ConfigurationTableName}";

        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(selectConfigurationQuery, connection);

        command.Connection.Open();

        using var reader = command.ExecuteReader();

        var configuration = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        while (reader.Read())
        {
            string key = reader[0].ToString() ?? throw new NullReferenceException("Key cannot be null");
            string? value = reader[1].ToString();
            configuration.Add(key, value);
        }

        return configuration;
    }

    /// <inheritdoc/>
    protected override byte[] GetCurrentVersion()
    {
        string selectCurrentVersionQuery = $"select top (1) {VersionFieldName} from {VersionTableName}";

        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(selectCurrentVersionQuery, connection);

        command.Connection.Open();

        return (byte[])command.ExecuteScalar();
    }

    /// <inheritdoc/>
    protected override bool IsVersioned() => VersionTableName is not null;
}
