using System.Collections;
using Microsoft.Data.SqlClient;

namespace ConfigurationRepository.SqlClient;

public sealed class SqlClientConfigurationRepository : IRepository
{
    private byte[]? _version;

    public string? ConnectionString { get; set; }

    public string? VersionTableName { get; set; }

    public string? ConfigurationTableName { get; set; }

    public IDictionary<string, string?> GetConfiguration()
    {
        ThrowIfPropertiesNotSet();
        CheckVersionInitialized();

        string GetConfigurationQuery = $"select [Key], [Value] from {ConfigurationTableName}";

        using var connection = new SqlConnection(ConnectionString);
        var query = new SqlCommand(GetConfigurationQuery, connection);

        query.Connection.Open();

        using var reader = query.ExecuteReader();

        var configuration = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        while (reader.Read())
        {
            string key = reader[0].ToString() ?? throw new NullReferenceException("Key cannot be null");
            string? value = reader[1].ToString();
            configuration.Add(key, value);
        }

        return configuration;
    }

    bool IRepository.IsReloadNeeded()
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

    private void CheckVersionInitialized()
    {
        if (_version is null && VersionTableName is not null)
        {
            _version = GetCurrentVersion();
        }
    }

    private byte[] GetCurrentVersion()
    {
        string GetCurrentVersionQuery = $"select top (1) [CurrentVersion] from {VersionTableName}";

        using var connection = new SqlConnection(ConnectionString);
        var query = new SqlCommand(GetCurrentVersionQuery, connection);

        query.Connection.Open();

        return (byte[])query.ExecuteScalar();
    }

    private void ThrowIfPropertiesNotSet()
    {
        _ = ConnectionString ?? throw new ArgumentNullException(nameof(ConnectionString));
        _ = ConfigurationTableName ?? throw new ArgumentNullException(nameof(ConfigurationTableName));
    }
}
