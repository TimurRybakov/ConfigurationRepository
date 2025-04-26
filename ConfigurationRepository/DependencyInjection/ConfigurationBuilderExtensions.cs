using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository;

/// <summary>
/// Extension methods for adding <see cref="ConfigurationRepositoryProvider"/>.
/// </summary>
public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// Adds a ConfigurationRepositorySource to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="configureSource">Configures the source.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddRepository<TSource>(
        this IConfigurationBuilder builder,
        IRepository repository,
        Action<TSource>? configureSource = null)
        where TSource : ConfigurationRepositorySource, new()
    {
        var source = new TSource();
        source.Repository = repository;
        configureSource?.Invoke(source);

        builder.Add(source);
        return builder;
    }

    /// <summary>
    /// Adds the ConfigurationRepositorySource to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="configureSource">Configures the source.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddRepository(
        this IConfigurationBuilder builder,
        IRepository repository,
        Action<ConfigurationRepositorySource>? configureSource = null)
    {
        return builder.AddRepository<ConfigurationRepositorySource>(repository, configureSource);
    }

    /// <summary>
    /// Sets a default action to be invoked for file-based providers when an error occurs.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="handler">The Action to be invoked on a database load exception.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder SetDatabaseLoadExceptionHandler(this IConfigurationBuilder builder,
        Action<RepositoryLoadExceptionContext> handler)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));

        builder.Properties[RepositoryLoadExceptionHandlerKey] = handler;
        return builder;
    }

    /// <summary>
    /// Gets a default action to be invoked for database-based providers when an error occurs.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
    /// <returns>The The Action to be invoked on a database load exception, if set.</returns>
    public static Action<RepositoryLoadExceptionContext>? GetDatabaseLoadExceptionHandler(this IConfigurationBuilder builder)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));

        if (builder.Properties.TryGetValue(RepositoryLoadExceptionHandlerKey, out object? handler))
        {
            return handler as Action<RepositoryLoadExceptionContext>;
        }
        return null;
    }

    /// <summary>
    /// Sets a configuration parser to be used parsing data being loaded from database.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="parser">The configuration parser to be used parsing load data from database.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder SetParsableConfigurationParser(this IConfigurationBuilder builder,
        IConfigurationParser parser)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));

        builder.Properties[ParsableConfigurationParserKey] = parser;
        return builder;
    }

    /// <summary>
    /// Gets a configuration parser to be used when parsing configuration data being loaded from database.
    /// If no parser is set then JsonConfigurationParser is used by default.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
    /// <returns>The configuration parser to be used parsing load data from database.</returns>
    public static IConfigurationParser GetParsableConfigurationParser(this IConfigurationBuilder builder)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));

        return (builder.Properties.TryGetValue(ParsableConfigurationParserKey, out object? parser)
            ? (IConfigurationParser)parser : null) ?? new JsonConfigurationParser();
    }

    /// <summary>
    /// Sets connection string that will be used to connect to the database.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="connectionString">The connection string to connect to the database.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder SetDatabaseConnectionString(this IConfigurationBuilder builder,
        string connectionString)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));

        builder.Properties[RepositoryDatabaseConnectionStringKey] = connectionString;
        return builder;
    }

    /// <summary>
    /// Gets connection string that will be used to connect to the database.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
    /// <returns>The connection string to connect to the database.</returns>
    public static string? GetDatabaseConnectionString(this IConfigurationBuilder builder)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));

        return builder.Properties.TryGetValue(RepositoryDatabaseConnectionStringKey, out object? connectionString)
            ? (string)connectionString : null;
    }

    /// <summary>
    /// Gets the <see cref="IRepository"/> that will be used to store configurations.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
    /// <returns>The <see cref="IRepository"/>.</returns>
    public static IRepository? GetConfigurationRepository(this IConfigurationBuilder builder)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));

        return (builder.Properties.TryGetValue(RepositoryKey, out object? repository))
            ? (IRepository)repository : null;
    }

    private const string RepositoryKey = "ConfigurationRepository:Repository";
    private const string RepositoryLoadExceptionHandlerKey = "ConfigurationRepository:Repository:LoadExceptionHandler";
    private const string ParsableConfigurationParserKey = "ConfigurationRepository:ParsableConfiguration:Parser";
    private const string RepositoryDatabaseConnectionStringKey = "ConfigurationRepository:Repository:DatabaseConnectionString";
}
