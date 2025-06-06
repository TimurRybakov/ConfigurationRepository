
namespace ConfigurationRepository.SqlClient;

public static class SqlClientJsonConfigurationRepositoryExtensions
{
    /// <summary>
    /// Sets ConnectionString for <paramref name="repository"/>.
    /// </summary>
    /// <param name="repository">Instance of <see cref="SqlClientParsableConfigurationRepository"/>.</param>
    /// <param name="connectionString">Connection string.</param>
    /// <returns>Instance of <see cref="SqlClientParsableConfigurationRepository"/>.</returns>
    public static SqlClientParsableConfigurationRepository UseConnectionString(
        this SqlClientParsableConfigurationRepository repository,
        string connectionString)
    {
        repository.ConnectionString = connectionString;
        return repository;
    }

    /// <summary>
    /// Sets ConfigurationTableName for <paramref name="repository"/>.
    /// </summary>
    /// <param name="repository">Instance of <see cref="SqlClientParsableConfigurationRepository"/>.</param>
    /// <param name="configurationTableName">Configuration table name.</param>
    /// <returns>Instance of <see cref="SqlClientParsableConfigurationRepository"/>.</returns>
    public static SqlClientParsableConfigurationRepository WithConfigurationTableName(
        this SqlClientParsableConfigurationRepository repository,
        string configurationTableName)
    {
        repository.ConfigurationTableName = configurationTableName;
        return repository;
    }

    /// <summary>
    /// Sets VersionFieldName for <paramref name="repository"/>.
    /// </summary>
    /// <param name="repository">Instance of <see cref="SqlClientParsableConfigurationRepository"/>.</param>
    /// <param name="versionFieldName">Configuration version field name.</param>
    /// <returns>Instance of <see cref="SqlClientParsableConfigurationRepository"/>.</returns>
    public static SqlClientParsableConfigurationRepository WithVersionFieldName(
        this SqlClientParsableConfigurationRepository repository,
        string? versionFieldName)
    {
        repository.VersionFieldName = versionFieldName;
        return repository;
    }

    /// <summary>
    /// Sets KeyFieldName for <paramref name="repository"/>.
    /// </summary>
    /// <param name="repository">Instance of <see cref="SqlClientParsableConfigurationRepository"/>.</param>
    /// <param name="keyFieldName">Configuration key field name.</param>
    /// <returns>Instance of <see cref="SqlClientParsableConfigurationRepository"/>.</returns>
    public static SqlClientParsableConfigurationRepository WithKeyFieldName(
        this SqlClientParsableConfigurationRepository repository,
        string keyFieldName)
    {
        repository.KeyFieldName = keyFieldName;
        return repository;
    }

    /// <summary>
    /// Sets ValueFieldName for <paramref name="repository"/>.
    /// </summary>
    /// <param name="repository">Instance of <see cref="SqlClientParsableConfigurationRepository"/>.</param>
    /// <param name="valueFieldName">Configuration value field name.</param>
    /// <returns>Instance of <see cref="SqlClientParsableConfigurationRepository"/>.</returns>
    public static SqlClientParsableConfigurationRepository WithValueFieldName(
        this SqlClientParsableConfigurationRepository repository,
        string valueFieldName)
    {
        repository.ValueFieldName = valueFieldName;
        return repository;
    }

    /// <summary>
    /// Sets Key for <paramref name="repository"/>. Assuming repository may contain different configurations accessed by it`s keys.
    /// </summary>
    /// <param name="repository">Instance of <see cref="SqlClientParsableConfigurationRepository"/>.</param>
    /// <param name="key">The configuration key.</param>
    /// <returns>Instance of <see cref="SqlClientParsableConfigurationRepository"/>.</returns>
    public static SqlClientParsableConfigurationRepository WithKey(
        this SqlClientParsableConfigurationRepository repository,
        string key)
    {
        repository.Key = key;
        return repository;
    }
}
