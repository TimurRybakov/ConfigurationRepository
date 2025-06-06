using System.Linq;
using Dapper;

namespace ConfigurationRepository.Dapper;

/// <summary>
/// A versioned dictionary repository that uses Dapper to fetch data from database.
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

    public override TData GetConfiguration<TData>()
    {
        return (TData)GetConfiguration();
    }

    private IDictionary<string, string?> GetConfiguration()
    {
        ThrowIfPropertiesNotSet();
        CheckVersionInitialized();

        using var connection = DbConnectionFactory!();

        var entries = connection.Query<ConfigurationEntry>(SelectConfigurationQuery!);

        return entries.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.OrdinalIgnoreCase);
    }

    protected override byte[]? GetCurrentVersion()
    {
        using var connection = DbConnectionFactory!();

        return (byte[]?)connection.ExecuteScalar(SelectCurrentVersionQuery!);
    }
}
