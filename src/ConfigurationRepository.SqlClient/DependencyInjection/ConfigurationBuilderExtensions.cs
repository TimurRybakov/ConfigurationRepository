using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository.SqlClient;

/// <summary>
/// Extension methods for <see cref="IConfigurationBuilder"/>.
/// </summary>
public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// Adds SqlClient dictionary configuration repository provider to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">An <see cref="IConfigurationBuilder"/> instance.</param>
    /// <param name="configureRepository">Configurator for <see cref="SqlClientDictionaryConfigurationRepository"/>.</param>
    /// <param name="configureSource">Configurator for <see cref="ConfigurationRepositorySource"/>.</param>
    /// <returns>An <see cref="IConfigurationBuilder"/> instance.</returns>
    public static IConfigurationBuilder AddSqlClientRepository(
        this IConfigurationBuilder builder,
        Action<SqlClientDictionaryConfigurationRepository> configureRepository,
        Action<ConfigurationRepositorySource>? configureSource = null)
    {
        var source = new ConfigurationRepositorySource();
        var repository = new SqlClientDictionaryConfigurationRepository();
        repository.TryAddConnectionString(builder);

        source.Repository = repository;

        configureRepository.Invoke(repository);
        configureSource?.Invoke(source);

        return builder.Add(source);
    }

    /// <summary>
    /// Adds SqlClient parsable json configuration repository provider to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">An <see cref="IConfigurationBuilder"/> instance.</param>
    /// <param name="configureRepository">Configurator for <see cref="SqlClientParsableConfigurationRepository"/>.</param>
    /// <param name="configureSource">Configurator for <see cref="ParsableConfigurationRepositorySource"/>.</param>
    /// <returns>An <see cref="IConfigurationBuilder"/> instance.</returns>
    public static IConfigurationBuilder AddSqlClientJsonRepository(
        this IConfigurationBuilder builder,
        Action<SqlClientParsableConfigurationRepository> configureRepository,
        Action<ParsableConfigurationRepositorySource>? configureSource = null)
    {
        var source = new ParsableConfigurationRepositorySource();
        var repository = new SqlClientParsableConfigurationRepository();
        repository.TryAddConnectionString(builder);

        source.Repository = repository;

        configureRepository.Invoke(repository);
        configureSource?.Invoke(source);

        return builder.Add(source);
    }

    private static SqlClientConfigurationRepository TryAddConnectionString(
        this SqlClientConfigurationRepository repository,
        IConfigurationBuilder builder)
    {
        var connectionString = builder.GetDatabaseConnectionString();
        if (connectionString is not null)
        {
            repository.ConnectionString = connectionString;
        }

        return repository;
    }
}
