
namespace ConfigurationRepository.SqlClient;

public static class SqlClientDictionaryConfigurationRepositoryExtensions
{
    public static TRepository UseConnectionString<TRepository>(
        this TRepository repository,
        string connectionString)
        where TRepository : SqlClientConfigurationRepository
    {
        repository.ConnectionString = connectionString;
        return repository;
    }

    public static TRepository WithConfigurationTableName<TRepository>(
        this TRepository repository,
        string configurationTableName)
        where TRepository : SqlClientConfigurationRepository
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
