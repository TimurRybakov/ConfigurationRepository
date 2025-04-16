using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository.SqlClient;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddSqlClientRepository(
        this IConfigurationBuilder builder,
        Action<SqlClientConfigurationRepository> configure,
        Action<SqlClientConfigurationSource>? configureSource = null)
    {
        var source = new SqlClientConfigurationSource();
        var repository = new SqlClientConfigurationRepository();

        source.Repository = repository;

        configure.Invoke(repository);
        configureSource?.Invoke(source);

        return builder.Add(source);
    }

    //public static IConfigurationBuilder AddEfCoreRepository(
    //this IConfigurationBuilder builder,
    //Action<ConfigurationRepositorySource> configure) =>
    //    AddEfCoreRepository(builder, (_, source) => configure(source));
}
