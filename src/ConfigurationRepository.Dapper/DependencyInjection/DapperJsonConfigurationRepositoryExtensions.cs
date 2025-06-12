
namespace ConfigurationRepository.Dapper;

public static class DapperJsonConfigurationRepositoryExtensions
{
    /// <summary>
    /// Sets the key that used to fetch configuration from repository.
    /// Assuming there may be several configurations in one repository.
    /// </summary>
    /// <param name="repository">An instance of <see cref="DapperConfigurationRepository"/> class.</param>
    /// <param name="key">A key string.</param>
    /// <returns>An instance of <see cref="DapperConfigurationRepository"/> class.</returns>
    public static DapperParsableConfigurationRepository WithKey(
        this DapperParsableConfigurationRepository repository,
        string key)
    {
        repository.Key = key;
        return repository;
    }
}
