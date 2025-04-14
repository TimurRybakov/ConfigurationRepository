
namespace ConfigurationRepository.SqlClient;

public static class SqlClientConfigurationRepositoryExtensions
{
    public static SqlClientConfigurationRepository UseConnectionString(
        this SqlClientConfigurationRepository repository,
        string connectionString)
    {
        repository.ConnectionString = connectionString;
        return repository;
    }

    public static SqlClientConfigurationRepository WithConfigurationTableName(
        this SqlClientConfigurationRepository repository,
        string configurationTableName)
    {
        repository.ConfigurationTableName = configurationTableName;
        return repository;
    }

    public static SqlClientConfigurationRepository WithVersionTableName(
        this SqlClientConfigurationRepository repository,
        string? versionTableName)
    {
        repository.VersionTableName = versionTableName;
        return repository;
    }
}
