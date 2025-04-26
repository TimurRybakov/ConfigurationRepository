using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.SqlClient;

namespace ConfigurationRepository.SqlClient;

public class SqlClientParsableConfigurationRepository :
    SqlClientConfigurationRepository, IVersionedRepository
{
    /// <summary>
    /// Version field name. If set, repository will check version change before reloading.
    /// </summary>
    public string? VersionFieldName { get; set; }

    /// <summary>
    /// The key value of <see cref="KeyFieldName"/> in <see cref="ConfigurationTableName"/> table that identifies desired configuration record.
    /// </summary>
    [DisallowNull]
    public string? Key { get; set; }

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

    protected override void ThrowIfPropertiesNotSet()
    {
        base.ThrowIfPropertiesNotSet();
        _ = Key ?? throw new ArgumentNullException(nameof(Key));
    }

    protected override bool IsVersioned() => VersionFieldName is not null;
}
