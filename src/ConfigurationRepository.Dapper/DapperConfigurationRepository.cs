using System.Collections;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace ConfigurationRepository.Dapper;

/// <summary>
/// A configuration storage repository in a database accessed using Dapper library.
/// </summary>
public abstract class DapperConfigurationRepository : VersionedRepository
{
    /// <summary>
    /// The database connection factory that creates <see cref="IDbConnection"/> using connection string.
    /// </summary>
    [DisallowNull]
    public Func<IDbConnection>? DbConnectionFactory { get; set; }

    /// <summary>
    /// Select SQL query that returns configuration. I.e. "select \"Key\", \"Value\" from appcfg.Configuration".
    /// </summary>
    [DisallowNull]
    public string? SelectConfigurationQuery { get; set; }

    /// <summary>
    /// SQL query to execute and return current configuration version. I.e. "select top (1) CurrentVersion from appcfg.Version".
    /// </summary>
    public string? SelectCurrentVersionQuery { get; set; }

    /// <summary>
    /// Checks if current <see cref="DapperConfigurationRepository"/> object state defines a versioned repository.
    /// Versioned repository has a select current version query.
    /// </summary>
    /// <returns>True if repository is versioned.</returns>
    protected override bool IsVersioned() => SelectCurrentVersionQuery is not null;

    /// <summary>
    /// Checks that all required fields are defined. Throws exceptions if not.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    protected virtual void ThrowIfPropertiesNotSet()
    {
        _ = DbConnectionFactory ?? throw new ArgumentNullException(nameof(DbConnectionFactory));
        _ = SelectConfigurationQuery ?? throw new ArgumentNullException(nameof(SelectConfigurationQuery));
    }
}
