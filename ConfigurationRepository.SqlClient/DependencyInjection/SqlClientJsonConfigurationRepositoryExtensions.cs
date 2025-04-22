
namespace ConfigurationRepository.SqlClient;

public static class SqlClientJsonConfigurationRepositoryExtensions
{
    public static SqlClientJsonConfigurationRepository UseConnectionString(
        this SqlClientJsonConfigurationRepository repository,
        string connectionString)
    {
        repository.ConnectionString = connectionString;
        return repository;
    }

    public static SqlClientJsonConfigurationRepository WithConfigurationTableName(
        this SqlClientJsonConfigurationRepository repository,
        string configurationTableName)
    {
        repository.ConfigurationTableName = configurationTableName;
        return repository;
    }

    public static SqlClientJsonConfigurationRepository WithVersionFieldName(
        this SqlClientJsonConfigurationRepository repository,
        string? versionFieldName)
    {
        repository.VersionFieldName = versionFieldName;
        return repository;
    }

    public static SqlClientJsonConfigurationRepository WithKeyFieldName(
    this SqlClientJsonConfigurationRepository repository,
    string keyFieldName)
    {
        repository.KeyFieldName = keyFieldName;
        return repository;
    }

    public static SqlClientJsonConfigurationRepository WithValueFieldName(
    this SqlClientJsonConfigurationRepository repository,
    string valueFieldName)
    {
        repository.ValueFieldName = valueFieldName;
        return repository;
    }

    public static SqlClientJsonConfigurationRepository WithKey(
    this SqlClientJsonConfigurationRepository repository,
    string? key)
    {
        repository.Key = key;
        return repository;
    }
}
