
using ConfigurationRepository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ParametrizedConfiguration;

public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// Builds parametrizable configuration, chains it to the builder, adds <see cref="ParametrizedConfigurationSource"/>
    /// to the builder and returns builded parametrizable configuration.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="parametrizableConfiguration"></param>
    /// <returns></returns>
    public static IConfigurationBuilder WithParametrization(
        this IConfigurationBuilder builder, out IConfiguration parametrizableConfiguration)
    {
        parametrizableConfiguration = builder.Build();
        var source = new ParametrizedConfigurationSource();
        source.Configuration = parametrizableConfiguration;

        builder.AddConfiguration(parametrizableConfiguration);
        builder.Add(source);

        return builder;
    }

    public static IConfigurationBuilder WithParametrization(this IConfigurationBuilder builder)
    {
        return WithParametrization(builder, out _);
    }

    public static IConfigurationBuilder WithParametrization<TService>(
        this IConfigurationBuilder builder,
        IServiceCollection services)
        where TService : class
    {
        builder.WithParametrization(out var parametrizableConfiguration);
        services.AddReloadableConfigurationService<TService>(parametrizableConfiguration);

        return builder;
    }
}
