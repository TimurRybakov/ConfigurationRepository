using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository.EntityFramework;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddEfCoreRepository(
        this IConfigurationBuilder builder,
        Action<DbContextOptionsBuilder> configureOptions,
        Action<ConfigurationRepositorySource>? configureSource = null)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RepositoryDbContext>();

        configureOptions.Invoke(optionsBuilder);

        return builder.AddEfCoreRepository(optionsBuilder.Options, configureSource);
    }

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

    public static IConfigurationBuilder AddEfCoreJsonRepository(
        this IConfigurationBuilder builder,
        string key,
        DbContextOptions<RepositoryDbContext> options,
        Action<ParsableConfigurationRepositorySource>? configureSource = null)
    {
        var source = new ParsableConfigurationRepositorySource();

        var repository =
            new EfParsableConfigurationRepository<RepositoryDbContext, ConfigurationEntry>(
                new RepositoryDbContext(options));

        repository.Key = key;

        source.Repository = repository;
        source.ConfigurationParser = new JsonConfigurationParser();

        configureSource?.Invoke(source);

        return builder.Add(source);
    }
}
