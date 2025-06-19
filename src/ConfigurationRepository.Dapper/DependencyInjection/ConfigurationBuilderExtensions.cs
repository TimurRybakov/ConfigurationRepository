using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository.Dapper;

/// <summary>
/// Extension methods for <see cref="IConfigurationBuilder"/>.
/// </summary>
public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// Adds dapper dictionary configuration repository provider to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">An <see cref="IConfigurationBuilder"/> instance.</param>
    /// <param name="configureRepository">Configurator for <see cref="DapperDictionaryConfigurationRepository"/>.</param>
    /// <param name="configureSource">Configurator for <see cref="ConfigurationRepositorySource"/>.</param>
    /// <returns>An <see cref="IConfigurationBuilder"/> instance.</returns>
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

        return builder.Add(source);
    }

    /// <summary>
    /// Adds dapper parsable json configuration repository provider to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">An <see cref="IConfigurationBuilder"/> instance.</param>
    /// <param name="configureRepository">Configurator for <see cref="DapperParsableConfigurationRepository"/>.</param>
    /// <param name="configureSource">Configurator for <see cref="ParsableConfigurationRepositorySource"/>.</param>
    /// <returns>An <see cref="IConfigurationBuilder"/> instance.</returns>
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

        return builder.Add(source);
    }
}
