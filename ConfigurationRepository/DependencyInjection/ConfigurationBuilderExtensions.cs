using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository;

/// <summary>
/// Extension methods for adding <see cref="ConfigurationRepositoryProvider"/>.
/// </summary>
public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// Adds the JSON configuration provider at <paramref name="path"/> to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="optional">Whether the configuration is optional.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    //public static IConfigurationBuilder AddStorage(
    //    this IConfigurationBuilder builder,
    //    bool optional = false)
    //{
    //    return AddStorage(
    //        builder,
    //        provider: null,
    //        optional: optional,
    //        reloadOnChange: false);
    //}

    /// <summary>
    /// Adds the JSON configuration provider at <paramref name="path"/> to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="optional">Whether the Configuration is optional.</param>
    /// <param name="reloadOnChange">Whether the configuration should be reloaded if the file changes.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    //public static IConfigurationBuilder AddStorage(
    //    this IConfigurationBuilder builder,
    //    bool optional,
    //    bool reloadOnChange)
    //{
    //    return AddStorage(
    //        builder,
    //        provider: null,
    //        optional,
    //        reloadOnChange);
    //}

    /// <summary>
    /// Adds a JSON configuration source to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="provider">The <see cref="IRepositoryChangeWatcher"/> to use to access the database.</param>
    /// <param name="configurationParser">Configuration parser to be used parsing data being loaded from database.</param>
    /// <param name="storedConfiguration">Configuration storage.</param>
    /// <param name="optional">Whether the Configuration is optional.</param>
    /// <param name="reloadOnChange">Whether the configuration should be reloaded if the database changes.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    //public static IConfigurationBuilder AddStorage(
    //    this IConfigurationBuilder builder,
    //    IDatabaseProvider? provider,
    //    IRepository? storedConfiguration,
    //    bool optional,
    //    bool reloadOnChange)
    //{
    //    _ = builder ?? throw new ArgumentNullException(nameof(builder));

    //    return builder.AddStorage(source =>
    //    {
    //        source.DatabaseProvider = provider;
    //        source.Repository = storedConfiguration;
    //        source.Optional = optional;
    //        source.ReloadOnChange = reloadOnChange;
    //    });
    //}

    /// <summary>
    /// Adds a JSON configuration source to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="configureSource">Configures the source.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddRepository(
        this IConfigurationBuilder builder,
        IRepository repository,
        Action<ConfigurationRepositorySource>? configureSource = null)
    {
        var source = new ConfigurationRepositorySource();
        source.Repository = repository;
        configureSource?.Invoke(source);

        builder.Add(source);
        return builder;
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
    //public static IConfigurationBuilder SetDatabaseConfigurationParser(this IConfigurationBuilder builder,
    //    IConfigurationParser parser)
    //{
    //    _ = builder ?? throw new ArgumentNullException(nameof(builder));

    //    builder.Properties[StorageConfigurationParserKey] = parser;
    //    return builder;
    //}

    /// <summary>
    /// Gets a configuration parser to be used when parsing configuration data being loaded from database.
    /// If no parser is set then JsonConfigurationParser is used by default.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
    /// <returns>The configuration parser to be used parsing load data from database.</returns>
    //public static IConfigurationParser GetDatabaseConfigurationParser(this IConfigurationBuilder builder)
    //{
    //    _ = builder ?? throw new ArgumentNullException(nameof(builder));

    //    return ((builder.Properties.TryGetValue(StorageConfigurationParserKey, out object? parser))
    //        ? (IConfigurationParser)parser : null) ?? new JsonConfigurationParser();
    //}

    /// <summary>
    /// Sets connection string that will be used to connect to the database.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="connectionString">The connection string to connect to the database.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    //public static IConfigurationBuilder SetDatabaseConnectionString(this IConfigurationBuilder builder,
    //    string connectionString)
    //{
    //    _ = builder ?? throw new ArgumentNullException(nameof(builder));

    //    builder.Properties[StorageKey] = connectionString;
    //    return builder;
    //}

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

    private const string RepositoryLoadExceptionHandlerKey = "StoredConfiguration:Repository:LoadExceptionHandler";
    //private const string StorageConfigurationParserKey = "StoredConfiguration:ConfigurationParser";
    private const string RepositoryKey = "StoredConfiguration:Repository";
}
