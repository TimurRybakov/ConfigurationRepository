using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.SqlClient;

namespace ConfigurationRepository.SqlClient;

public sealed class SqlClientDictionaryConfigurationRepository : SqlClientConfigurationRepository, IVersionedRepository
{
    private byte[]? _version;

    /// <summary>
    /// A table containing current configuration version. If set, repository will check version change before reloading.
    /// </summary>
    public string? VersionTableName { get; set; }

    /// <summary>
    /// Table name with configuration.
    /// </summary>
    [DisallowNull]
    public string? ConfigurationTableName { get; set; }

    public IRetrievalStrategy RetrievalStrategy { get; }

    public SqlClientDictionaryConfigurationRepository()
    {
        RetrievalStrategy = DictionaryRetrievalStrategy.Instance;
    }

    public TData GetConfiguration<TData>()
    {
        return (TData)GetConfiguration();
    }

    public bool VersionChanged()
    {
        if (VersionTableName is null)
            return true;

        var newVersion = GetCurrentVersion();

        if (StructuralComparisons.StructuralComparer.Compare(newVersion, _version) == 0)
        {
            return false;
        }

        _version = newVersion;
        return true;
    }

    private IDictionary<string, string?> GetConfiguration()
    {
        ThrowIfPropertiesNotSet();
        CheckVersionInitialized();

        string selectConfigurationQuery = $"select [Key], [Value] from {ConfigurationTableName}";

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

    private void CheckVersionInitialized()
    {
        if (_version is null && VersionTableName is not null)
        {
            _version = GetCurrentVersion();
        }
    }

    private byte[] GetCurrentVersion()
    {
        string selectCurrentVersionQuery = $"select top (1) [CurrentVersion] from {VersionTableName}";

        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(selectCurrentVersionQuery, connection);

        command.Connection.Open();

        return (byte[])command.ExecuteScalar();
    }

    private void ThrowIfPropertiesNotSet()
    {
        _ = ConnectionString ?? throw new ArgumentNullException(nameof(ConnectionString));
        _ = ConfigurationTableName ?? throw new ArgumentNullException(nameof(ConfigurationTableName));
    }
}
