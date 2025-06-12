
using System.Data;

namespace ConfigurationRepository.Dapper;

public static class DapperDictionaryConfigurationRepositoryExtensions
{
    /// <summary>
    /// Sets DbConnectionFactory property for <paramref name="repository"/>.
    /// Сonnection factory is used to create <see cref="IDbConnection"/> using <see cref="ConnectionString"/>.
    /// </summary>
    /// <typeparam name="TRepository">A generic type of <see cref="DapperConfigurationRepository"/> class.</typeparam>
    /// <param name="repository">An instance of <see cref="DapperConfigurationRepository"/> class or it`s descendant.</param>
    /// <param name="dbConnectionFactory">A factory method that creates database connection of type <see cref="IDbConnection"/>.</param>
    /// <returns>An instance of <see cref="DapperConfigurationRepository"/> class or it`s descendant.</returns>
    public static TRepository UseDbConnectionFactory<TRepository>(
        this TRepository repository,
        Func<IDbConnection> dbConnectionFactory)
        where TRepository : DapperConfigurationRepository
    {
        repository.DbConnectionFactory = dbConnectionFactory;
        return repository;
    }

    /// <summary>
    /// Sets SelectConfigurationQuery property for <paramref name="repository"/>.
    /// Select SQL query that returns configuration. I.e. "select \"Key\", \"Value\" from appcfg.Configuration".
    /// </summary>
    /// <typeparam name="TRepository">A generic type of <see cref="DapperConfigurationRepository"/> class.</typeparam>
    /// <param name="repository">An instance of <see cref="DapperConfigurationRepository"/> class or it`s descendant.</param>
    /// <param name="selectConfigurationQuery">The select query.</param>
    /// <returns>An instance of <see cref="DapperConfigurationRepository"/> class or it`s descendant.</returns>
    public static TRepository WithSelectConfigurationQuery<TRepository>(
        this TRepository repository,
        string selectConfigurationQuery)
        where TRepository : DapperConfigurationRepository
    {
        repository.SelectConfigurationQuery = selectConfigurationQuery;
        return repository;
    }

    /// <summary>
    /// Sets SelectCurrentVersionQuery property for <paramref name="repository"/>.
    /// SQL query to execute and return current configuration version. I.e. "select top (1) CurrentVersion from appcfg.Version".
    /// </summary>
    /// <typeparam name="TRepository">A generic type of <see cref="DapperConfigurationRepository"/> class.</typeparam>
    /// <param name="repository">An instance of <see cref="DapperConfigurationRepository"/> class or it`s descendant.</param>
    /// <param name="selectCurrentVersionQuery">The select query.</param>
    /// <returns>An instance of <see cref="DapperConfigurationRepository"/> class or it`s descendant.</returns>
    public static TRepository WithSelectCurrentVersionQuery<TRepository>(
        this TRepository repository,
        string? selectCurrentVersionQuery)
        where TRepository : DapperConfigurationRepository
    {
        repository.SelectCurrentVersionQuery = selectCurrentVersionQuery;
        return repository;
    }
}
