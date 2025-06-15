using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository.EntityFramework;

/// <summary>
/// Extension methods for <see cref="IConfigurationBuilder"/>.
/// </summary>
public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// Adds EF Core dictionary configuration repository provider to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">An <see cref="IConfigurationBuilder"/> instance.</param>
    /// <param name="configureOptions">Configurator for <see cref="DbContextOptionsBuilder"/></param>
    /// <param name="configureSource">Configurator for <see cref="ConfigurationRepositorySource"/>.</param>
    /// <returns>An <see cref="IConfigurationBuilder"/> instance.</returns>
    public static IConfigurationBuilder AddEfCoreRepository(
        this IConfigurationBuilder builder,
        Action<DbContextOptionsBuilder> configureOptions,
        Action<ConfigurationRepositorySource>? configureSource = null)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RepositoryDbContext>();

        configureOptions.Invoke(optionsBuilder);

        return builder.AddEfCoreRepository(optionsBuilder.Options, configureSource);
    }

    /// <summary>
    /// Adds EF Core dictionary configuration repository provider to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">An <see cref="IConfigurationBuilder"/> instance.</param>
    /// <param name="options">Preconfigured database context options.</param>
    /// <param name="configureSource">Configurator for <see cref="ConfigurationRepositorySource"/>.</param>
    /// <returns>An <see cref="IConfigurationBuilder"/> instance.</returns>
    public static IConfigurationBuilder AddEfCoreRepository(
        this IConfigurationBuilder builder,
        DbContextOptions<RepositoryDbContext> options,
        Action<ConfigurationRepositorySource>? configureSource = null)
    {
        var source = new ConfigurationRepositorySource();

        var repository =
            new EfDictionaryConfigurationRepository<RepositoryDbContext, ConfigurationEntry>(
                new RepositoryDbContext(options));

        source.Repository = repository;
        source.RetrievalStrategy = DictionaryRetrievalStrategy.Instance;

        configureSource?.Invoke(source);

        return builder.Add(source);
    }

    /// <summary>
    /// Adds EF Core json parsable configuration repository provider to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">An <see cref="IConfigurationBuilder"/> instance.</param>
    /// <param name="key">A key string.</param>
    /// <param name="configureOptions">Configurator for <see cref="DbContextOptionsBuilder"/></param>
    /// <param name="configureSource">Configurator for <see cref="ConfigurationRepositorySource"/>.</param>
    /// <returns>An <see cref="IConfigurationBuilder"/> instance.</returns>
    public static IConfigurationBuilder AddEfCoreJsonRepository(
        this IConfigurationBuilder builder,
        string key,
        Action<DbContextOptionsBuilder> configureOptions,
        Action<ConfigurationRepositorySource>? configureSource = null)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RepositoryDbContext>();

        configureOptions.Invoke(optionsBuilder);

        return builder.AddEfCoreJsonRepository(key, optionsBuilder.Options, configureSource);
    }

    /// <summary>
    /// Adds EF Core json parsable configuration repository provider to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">An <see cref="IConfigurationBuilder"/> instance.</param>
    /// <param name="key">A key string.</param>
    /// <param name="options">Preconfigured database context options.</param>
    /// <param name="configureSource">Configurator for <see cref="ConfigurationRepositorySource"/>.</param>
    /// <returns>An <see cref="IConfigurationBuilder"/> instance.</returns>
    public static IConfigurationBuilder AddEfCoreJsonRepository(
        this IConfigurationBuilder builder,
        string key,
        DbContextOptions<RepositoryDbContext> options,
        Action<ParsableConfigurationRepositorySource>? configureSource = null)
    {
        var source = new ParsableConfigurationRepositorySource();

        var repository =
            new EfParsableConfigurationRepository<RepositoryDbContext, ConfigurationEntry>(
                new RepositoryDbContext(options))
            {
                Key = key
            };

        source.Repository = repository;
        source.ConfigurationParser = new JsonConfigurationParser();

        configureSource?.Invoke(source);

        return builder.Add(source);
    }
}
