using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository.SqlClient;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddSqlClientRepository(
        this IConfigurationBuilder builder,
        Action<SqlClientDictionaryConfigurationRepository> configureRepository,
        Action<ConfigurationRepositorySource>? configureSource = null)
    {
        var source = new ConfigurationRepositorySource();
        var repository = new SqlClientDictionaryConfigurationRepository();

        source.Repository = repository;

        configureRepository.Invoke(repository);
        configureSource?.Invoke(source);

        source.RetrievalStrategy ??= DictionaryRetrievalStrategy.Instance;

        return builder.Add(source);
    }

    public static IConfigurationBuilder AddSqlClientJsonRepository(
        this IConfigurationBuilder builder,
        Action<SqlClientParsableConfigurationRepository> configureRepository,
        Action<ParsableConfigurationRepositorySource>? configureSource = null)
    {
        var source = new ParsableConfigurationRepositorySource();
        var repository = new SqlClientParsableConfigurationRepository();

        source.Repository = repository;

        configureRepository.Invoke(repository);
        configureSource?.Invoke(source);

        source.ConfigurationParser ??= new JsonConfigurationParser();

        return builder.Add(source);
    }
}
