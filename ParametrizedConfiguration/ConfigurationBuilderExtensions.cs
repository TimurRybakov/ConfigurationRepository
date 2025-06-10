
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
    /// <param name="builder">A configuration builder instance to wich we add <see cref="ParametrizedConfigurationSource"/>.</param>
    /// <param name="parametrizableConfiguration">Created instance of parametrizable configuration.
    /// We return this to use it elsewere, i.e. in configuration reloader service.</param>
    /// <param name="configureSource">Configures the source.</param>
    /// <returns>A configuration builder instance.</returns>
    public static IConfigurationBuilder WithParametrization(
        this IConfigurationBuilder builder,
        out IConfiguration parametrizableConfiguration,
        Action<ParametrizedConfigurationSource>? configureSource = null)
    {
        parametrizableConfiguration = builder.Build();
        var source = new ParametrizedConfigurationSource();
        source.Configuration = parametrizableConfiguration;

        configureSource?.Invoke(source);

        builder.AddConfiguration(parametrizableConfiguration);
        builder.Add(source);

        return builder;
    }

    public static IConfigurationBuilder WithParametrization(
        this IConfigurationBuilder builder,
        Action<ParametrizedConfigurationSource>? configureSource = null)
    {
        return WithParametrization(builder, out _, configureSource);
    }

    /// <summary>
    /// Builds parametrizable configuration, chains it to the builder, adds <see cref="ParametrizedConfigurationSource"/>
    /// to the builder and registers builded parametrizable configuration as <see cref="IReloadableConfigurationService{TService}"/>.
    /// </summary>
    /// <typeparam name="TService">A generic type of a marker class for <see cref="IReloadableConfigurationService"/>.</typeparam>
    /// <param name="builder">A configuration builder instance to wich we add <see cref="ParametrizedConfigurationSource"/>.</param>
    /// <param name="services">A service collection for reloadable service registration.</param>
    /// <returns></returns>
    public static IConfigurationBuilder WithParametrization<TService>(
        this IConfigurationBuilder builder,
        IServiceCollection services,
        Action<ParametrizedConfigurationSource>? configureSource = null)
        where TService : class
    {
        builder.WithParametrization(out var parametrizableConfiguration, configureSource);
        services.AddReloadableConfigurationService<TService>(parametrizableConfiguration);

        return builder;
    }
}
