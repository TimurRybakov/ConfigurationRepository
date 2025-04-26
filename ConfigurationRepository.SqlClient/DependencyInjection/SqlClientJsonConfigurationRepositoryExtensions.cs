
namespace ConfigurationRepository.SqlClient;

public static class SqlClientJsonConfigurationRepositoryExtensions
{
    public static SqlClientParsableConfigurationRepository UseConnectionString(
        this SqlClientParsableConfigurationRepository repository,
        string connectionString)
    {
        repository.ConnectionString = connectionString;
        return repository;
    }

    public static SqlClientParsableConfigurationRepository WithConfigurationTableName(
        this SqlClientParsableConfigurationRepository repository,
        string configurationTableName)
    {
        repository.ConfigurationTableName = configurationTableName;
        return repository;
    }

    public static SqlClientParsableConfigurationRepository WithVersionFieldName(
        this SqlClientParsableConfigurationRepository repository,
        string? versionFieldName)
    {
        repository.VersionFieldName = versionFieldName;
        return repository;
    }

    public static SqlClientParsableConfigurationRepository WithKeyFieldName(
        this SqlClientParsableConfigurationRepository repository,
    string keyFieldName)
    {
        repository.KeyFieldName = keyFieldName;
        return repository;
    }

    public static SqlClientParsableConfigurationRepository WithValueFieldName(
        this SqlClientParsableConfigurationRepository repository,
    string valueFieldName)
    {
        repository.ValueFieldName = valueFieldName;
        return repository;
    }

    public static SqlClientParsableConfigurationRepository WithKey(
        this SqlClientParsableConfigurationRepository repository,
    string key)
    {
        repository.Key = key;
        return repository;
    }
}
