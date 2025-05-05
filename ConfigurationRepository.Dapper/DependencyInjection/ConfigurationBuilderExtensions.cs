using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository.Dapper;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddDapperRepository(
        this IConfigurationBuilder builder,
        Action<DapperDictionaryConfigurationRepository> configureRepository,
        Action<ConfigurationRepositorySource>? configureSource = null)
    {
        var source = new ConfigurationRepositorySource();
        var repository = new DapperDictionaryConfigurationRepository();

        source.Repository = repository;

        configureRepository.Invoke(repository);
        configureSource?.Invoke(source);

        source.RetrievalStrategy ??= DictionaryRetrievalStrategy.Instance;

        return builder.Add(source);
    }

    public static IConfigurationBuilder AddDapperJsonRepository(
        this IConfigurationBuilder builder,
        Action<DapperParsableConfigurationRepository> configureRepository,
        Action<ParsableConfigurationRepositorySource>? configureSource = null)
    {
        var source = new ParsableConfigurationRepositorySource();
        var repository = new DapperParsableConfigurationRepository();

        source.Repository = repository;

        configureRepository.Invoke(repository);
        configureSource?.Invoke(source);

        source.ConfigurationParser ??= new JsonConfigurationParser();

        return builder.Add(source);
    }
}
