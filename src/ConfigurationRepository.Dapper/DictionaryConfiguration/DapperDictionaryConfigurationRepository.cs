using Dapper;

namespace ConfigurationRepository.Dapper;

/// <summary>
/// A versioned dictionary repository that uses Dapper to fetch data from database.
/// Configuration is stored in key-value pairs.
/// </summary>
public sealed class DapperDictionaryConfigurationRepository : DapperConfigurationRepository
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

        using var connection = DbConnectionFactory!();

        var entries = connection.Query<ConfigurationEntry>(SelectConfigurationQuery!);

        return entries.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    protected override byte[]? GetCurrentVersion()
    {
        using var connection = DbConnectionFactory!();

        return (byte[]?)connection.ExecuteScalar(SelectCurrentVersionQuery!);
    }
}
