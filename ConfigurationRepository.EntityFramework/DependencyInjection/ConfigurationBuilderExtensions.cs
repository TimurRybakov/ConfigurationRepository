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
            new EfConfigurationRepository<RepositoryDbContext, ConfigurationEntry>(
                new RepositoryDbContext(options));

        source.Repository = repository;

        configureSource?.Invoke(source);

        return builder.Add(source);
    }
}
