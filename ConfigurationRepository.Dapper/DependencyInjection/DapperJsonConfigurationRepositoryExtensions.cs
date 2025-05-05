
namespace ConfigurationRepository.Dapper;

public static class DapperJsonConfigurationRepositoryExtensions
{

    public static DapperParsableConfigurationRepository WithKey(
        this DapperParsableConfigurationRepository repository,
        string key)
    {
        repository.Key = key;
        return repository;
    }
}
