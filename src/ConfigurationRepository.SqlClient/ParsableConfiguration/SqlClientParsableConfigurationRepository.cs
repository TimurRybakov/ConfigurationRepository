using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.SqlClient;

namespace ConfigurationRepository.SqlClient;

/// <summary>
/// A versioned parsable repository that uses SqlClient to fetch data from database.
/// </summary>
public class SqlClientParsableConfigurationRepository :
    SqlClientConfigurationRepository, IVersionedRepository
{
    /// <summary>
    /// Version field name. If set, repository will check version change before reloading.
    /// </summary>
    public string? VersionFieldName { get; set; }

    /// <summary>
    /// Assuming there are several configurations in the database, this key is used to fetch exact one of them.
    /// </summary>
    [DisallowNull]
    public string? Key { get; set; }

    /// <inheritdoc/>
    public override TData GetConfiguration<TData>()
    {
        return (TData)Convert.ChangeType(GetConfiguration(), typeof(TData));
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

    /// <inheritdoc/>
    protected override byte[]? GetCurrentVersion()
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

    /// <inheritdoc/>
    protected override void ThrowIfPropertiesNotSet()
    {
        base.ThrowIfPropertiesNotSet();
        _ = Key ?? throw new ArgumentNullException(nameof(Key));
    }

    /// <inheritdoc/>
    protected override bool IsVersioned() => VersionFieldName is not null;
}
