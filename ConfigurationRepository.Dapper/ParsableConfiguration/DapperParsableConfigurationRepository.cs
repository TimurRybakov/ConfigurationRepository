using System.Diagnostics.CodeAnalysis;
using Dapper;

namespace ConfigurationRepository.Dapper;

public class DapperParsableConfigurationRepository :
    DapperConfigurationRepository, IVersionedRepository
{
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

        using var connection = DbConnectionFactory!();

        return connection.ExecuteScalar<string>(SelectConfigurationQuery!, new { Key })
            ?? throw new InvalidOperationException("Null configuration returned from server.");
    }

    protected override byte[]? GetCurrentVersion()
    {
        using var connection = DbConnectionFactory!();

        return connection.ExecuteScalar<byte[]?>(SelectCurrentVersionQuery!, new { Key });
    }

    protected override void ThrowIfPropertiesNotSet()
    {
        base.ThrowIfPropertiesNotSet();
        _ = Key ?? throw new ArgumentNullException(nameof(Key));
    }
}
