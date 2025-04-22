using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository.SqlClient;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddSqlClientDictionaryRepository(
        this IConfigurationBuilder builder,
        Action<SqlClientDictionaryConfigurationRepository> configureRepository,
        Action<SqlClientDictionaryConfigurationSource>? configureSource = null)
    {
        var source = new SqlClientDictionaryConfigurationSource();
        var repository = new SqlClientDictionaryConfigurationRepository();

        source.Repository = repository;

        configureRepository.Invoke(repository);
        configureSource?.Invoke(source);

        return builder.Add(source);
    }

    public static IConfigurationBuilder AddSqlClientJsonRepository(
        this IConfigurationBuilder builder,
        Action<SqlClientJsonConfigurationRepository> configureRepository,
        Action<SqlClientJsonConfigurationSource>? configureSource = null)
    {
        var source = new SqlClientJsonConfigurationSource();
        var repository = new SqlClientJsonConfigurationRepository();

        source.Repository = repository;

        configureRepository.Invoke(repository);
        configureSource?.Invoke(source);

        return builder.Add(source);
    }
}
