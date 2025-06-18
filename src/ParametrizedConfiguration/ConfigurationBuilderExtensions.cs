
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConfigurationRepository;

/// <summary>
/// Extension methods for <see cref="IConfigurationBuilder"/>.
/// </summary>
public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// Builds parametrizable configuration, chains it to the builder, adds <see cref="ParametrizedConfigurationSource"/>
    /// to the builder and returns builded parametrizable configuration.
    /// </summary>
    /// <param name="builder">A configuration builder instance to wich we add <see cref="ParametrizedConfigurationSource"/>.</param>
    /// <param name="parametrizableConfiguration">Created instance of parametrizable configuration.
    /// We return this to use it elsewere, i.e. in configuration reloader service.</param>
    /// <param name="configureSource">Configures the <see cref="ParametrizedConfigurationSource"/>, if set.</param>
    /// <returns>A configuration <paramref name="builder"/> instance.</returns>
    public static IConfigurationBuilder WithParametrization(
        this IConfigurationBuilder builder,
        out IConfiguration parametrizableConfiguration,
        Action<ParametrizedConfigurationSource>? configureSource = null)
    {
        parametrizableConfiguration = builder.Build();
        var source = new ParametrizedConfigurationSource
        {
            Configuration = parametrizableConfiguration
        };

        configureSource?.Invoke(source);

        builder.AddConfiguration(parametrizableConfiguration);
        builder.Add(source);

        return builder;
    }

    /// <summary>
    /// Builds parametrizable configuration, chains it to the builder and adds <see cref="ParametrizedConfigurationSource"/> to the builder.
    /// </summary>
    /// <param name="builder">A configuration builder instance to wich we add <see cref="ParametrizedConfigurationSource"/>.</param>
    /// <param name="configureSource">Configures the <see cref="ParametrizedConfigurationSource"/>, if set.</param>
    /// <returns>A configuration <paramref name="builder"/> instance.</returns>
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
    /// <param name="configureSource">Configures the <see cref="ParametrizedConfigurationSource"/>, if set.</param>
    /// <returns>A configuration <paramref name="builder"/> instance.</returns>
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

    /// <summary>
    /// Builds parametrizable configuration, chains it to the builder, adds <see cref="ParametrizedConfigurationSource"/>
    /// to the builder and registers builded parametrizable configuration as <see cref="IReloadableConfigurationService{TService}"/> with default marker class.
    /// </summary>
    /// <param name="builder">A configuration builder instance to wich we add <see cref="ParametrizedConfigurationSource"/>.</param>
    /// <param name="services">A service collection for reloadable service registration.</param>
    /// <param name="configureSource">Configures the <see cref="ParametrizedConfigurationSource"/>, if set.</param>
    /// <returns>A configuration <paramref name="builder"/> instance.</returns>
    public static IConfigurationBuilder WithParametrization(
        this IConfigurationBuilder builder,
        IServiceCollection services,
        Action<ParametrizedConfigurationSource>? configureSource = null)
    {
        builder.WithParametrization(out var parametrizableConfiguration, configureSource);
        services.AddReloadableConfigurationService(parametrizableConfiguration);

        return builder;
    }
}
