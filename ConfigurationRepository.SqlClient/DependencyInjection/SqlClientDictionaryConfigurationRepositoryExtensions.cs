
namespace ConfigurationRepository.SqlClient;

public static class SqlClientDictionaryConfigurationRepositoryExtensions
{
    /// <summary>
    /// Sets ConnectionString property for <paramref name="repository"/>.
    /// Сonnection string is used to create <see cref="IDbConnection"/>.
    /// </summary>
    /// <typeparam name="TRepository">A generic type of <see cref="SqlClientConfigurationRepository"/> class.</typeparam>
    /// <param name="repository">An instance of <see cref="SqlClientConfigurationRepository"/> class or it`s descendant.</param>
    /// <param name="dbConnectionFactory">A factory method that creates database connection of type <see cref="IDbConnection"/>.</param>
    /// <returns>An instance of <see cref="SqlClientConfigurationRepository"/> class or it`s descendant.</returns>

    public static TRepository UseConnectionString<TRepository>(
        this TRepository repository,
        string connectionString)
        where TRepository : SqlClientConfigurationRepository
    {
        repository.ConnectionString = connectionString;
        return repository;
    }

    /// <summary>
    /// Sets ConfigurationTableName property for <paramref name="repository"/>.
    /// Configuration table name is used in select SQL query that returns configuration.
    /// I.e. "select \"Key\", \"Value\" from appcfg.Configuration".
    /// </summary>
    /// <typeparam name="TRepository">A generic type of <see cref="SqlClientConfigurationRepository"/> class.</typeparam>
    /// <param name="repository">An instance of <see cref="SqlClientConfigurationRepository"/> class or it`s descendant.</param>
    /// <param name="configurationTableName">Configuration table name.</param>
    /// <returns>An instance of <see cref="SqlClientConfigurationRepository"/> class or it`s descendant.</returns>
    public static TRepository WithConfigurationTableName<TRepository>(
        this TRepository repository,
        string configurationTableName)
        where TRepository : SqlClientConfigurationRepository
    {
        repository.ConfigurationTableName = configurationTableName;
        return repository;
    }

    /// <summary>
    /// Sets VersionTableName property for <paramref name="repository"/>.
    /// Configuration version table name is used in select SQL query that returns current configuration version.
    /// I.e. "select top (1) CurrentVersion from appcfg.Version".
    /// </summary>
    /// <param name="repository">An instance of <see cref="SqlClientDictionaryConfigurationRepository"/> class.</param>
    /// <param name="versionTableName">Configuration version table name.</param>
    /// <returns>An instance of <see cref="DapperConfigurationRepository"/> class or it`s descendant.</returns>
    public static SqlClientDictionaryConfigurationRepository WithVersionTableName(
        this SqlClientDictionaryConfigurationRepository repository,
        string? versionTableName)
    {
        repository.VersionTableName = versionTableName;
        return repository;
    }
}
