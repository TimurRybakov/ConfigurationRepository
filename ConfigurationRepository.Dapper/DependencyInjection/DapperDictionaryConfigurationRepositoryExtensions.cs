
using System.Data;
using ConfigurationRepository.Dapper;

namespace ConfigurationRepository.Dapper;

public static class DapperDictionaryConfigurationRepositoryExtensions
{
    public static TRepository UseDbConnectionFactory<TRepository>(
        this TRepository repository,
        Func<IDbConnection> dbConnectionFactory)
        where TRepository : DapperConfigurationRepository
    {
        repository.DbConnectionFactory = dbConnectionFactory;
        return repository;
    }

    public static TRepository WithSelectConfigurationQuery<TRepository>(
        this TRepository repository,
        string selectConfigurationQuery)
        where TRepository : DapperConfigurationRepository
    {
        repository.SelectConfigurationQuery = selectConfigurationQuery;
        return repository;
    }

    public static TRepository WithSelectCurrentVersionQuery<TRepository>(
        this TRepository repository,
        string? selectCurrentVersionQuery)
        where TRepository : DapperConfigurationRepository
    {
        repository.SelectCurrentVersionQuery = selectCurrentVersionQuery;
        return repository;
    }
}
