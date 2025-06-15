using System.Diagnostics.CodeAnalysis;
using Dapper;

namespace ConfigurationRepository.Dapper;

/// <summary>
/// A versioned parsable repository that uses Dapper to fetch data from database.
/// </summary>
public class DapperParsableConfigurationRepository :
    DapperConfigurationRepository, IVersionedRepository
{
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

        using var connection = DbConnectionFactory!();

        return connection.ExecuteScalar<string>(SelectConfigurationQuery!, new { Key })
            ?? throw new InvalidOperationException("Null configuration returned from server.");
    }

    /// <inheritdoc/>
    protected override byte[]? GetCurrentVersion()
    {
        using var connection = DbConnectionFactory!();

        return connection.ExecuteScalar<byte[]?>(SelectCurrentVersionQuery!, new { Key });
    }

    /// <inheritdoc/>
    protected override void ThrowIfPropertiesNotSet()
    {
        base.ThrowIfPropertiesNotSet();
        _ = Key ?? throw new ArgumentNullException(nameof(Key));
    }
}
