
namespace ConfigurationRepository.SqlClient;

public static class SqlClientDictionaryConfigurationRepositoryExtensions
{
    public static SqlClientDictionaryConfigurationRepository UseConnectionString(
        this SqlClientDictionaryConfigurationRepository repository,
        string connectionString)
    {
        repository.ConnectionString = connectionString;
        return repository;
    }

    public static SqlClientDictionaryConfigurationRepository WithConfigurationTableName(
        this SqlClientDictionaryConfigurationRepository repository,
        string configurationTableName)
    {
        repository.ConfigurationTableName = configurationTableName;
        return repository;
    }

    public static SqlClientDictionaryConfigurationRepository WithVersionTableName(
        this SqlClientDictionaryConfigurationRepository repository,
        string? versionTableName)
    {
        repository.VersionTableName = versionTableName;
        return repository;
    }
}
