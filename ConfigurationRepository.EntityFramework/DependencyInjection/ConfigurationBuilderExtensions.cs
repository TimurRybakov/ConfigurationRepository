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
        var optionsBuilder = new DbContextOptionsBuilder<DictionaryRepositoryDbContext>();

        configureOptions.Invoke(optionsBuilder);

        return builder.AddEfCoreRepository(optionsBuilder.Options, configureSource);
    }

    public static IConfigurationBuilder AddEfCoreRepository(
        this IConfigurationBuilder builder,
        DbContextOptions<DictionaryRepositoryDbContext> options,
        Action<ConfigurationRepositorySource>? configureSource = null)
    {
        var source = new ConfigurationRepositorySource();

        var repository =
            new EfDictionaryConfigurationRepository<DictionaryRepositoryDbContext, DictionaryConfigurationEntry>(
                new DictionaryRepositoryDbContext(options));

        source.Repository = repository;

        configureSource?.Invoke(source);

        return builder.Add(source);
    }
}
