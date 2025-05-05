using System.Collections;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace ConfigurationRepository.Dapper;

public abstract class DapperConfigurationRepository : IVersionedRepository
{
    protected byte[]? _version;

    /// <summary>
    /// The connection factory that creates <see cref="IDbConnection"/> using <see cref="ConnectionString"/>.
    /// </summary>
    [DisallowNull]
    public Func<IDbConnection>? DbConnectionFactory { get; set; }

    /// <summary>
    /// SQL query to execute and return configuration. I.e. "select \"Key\", \"Value\" from appcfg.Configuration".
    /// </summary>
    [DisallowNull]
    public string? SelectConfigurationQuery { get; set; }


    /// <summary>
    /// SQL query to execute and return current configuration version. I.e. "select top (1) CurrentVersion from appcfg.Version".
    /// </summary>
    public string? SelectCurrentVersionQuery { get; set; }

    public abstract TData GetConfiguration<TData>();

    public bool VersionChanged()
    {
        if (!IsVersioned())
            return true;

        var newVersion = GetCurrentVersion();

        if (StructuralComparisons.StructuralComparer.Compare(newVersion, _version) == 0)
        {
            return false;
        }

        _version = newVersion;
        return true;
    }

    protected abstract byte[]? GetCurrentVersion();

    protected bool IsVersioned() => SelectCurrentVersionQuery is not null;

    protected void CheckVersionInitialized()
    {
        if (_version is null && IsVersioned())
        {
            _version = GetCurrentVersion();
        }
    }

    protected virtual void ThrowIfPropertiesNotSet()
    {
        _ = DbConnectionFactory ?? throw new ArgumentNullException(nameof(DbConnectionFactory));
        _ = SelectConfigurationQuery ?? throw new ArgumentNullException(nameof(SelectConfigurationQuery));
    }
}
