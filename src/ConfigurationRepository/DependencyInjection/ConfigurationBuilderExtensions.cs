using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository;

/// <summary>
/// Extension methods for adding <see cref="ConfigurationRepositoryProvider"/>.
/// </summary>
public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// Adds an <see cref="IConfigurationRepository"/> object to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">A configuration builder instance for adding <see cref="IConfigurationRepository"/> or it`s descendant.</param>
    /// <param name="repository">An <see cref="IConfigurationRepository"/> object.</param>
    /// <param name="configureSource">If set, configures <see cref="ConfigurationRepositorySource"/>.</param>
    /// <returns>An <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddRepository<TSource>(
        this IConfigurationBuilder builder,
        IConfigurationRepository repository,
        Action<TSource>? configureSource = null)
        where TSource : ConfigurationRepositorySource, new()
    {
        var source = new TSource
        {
            Repository = repository
        };
        configureSource?.Invoke(source);

        builder.Add(source);
        return builder;
    }

    /// <summary>
    /// Adds an <see cref="IConfigurationRepository"/> object to <paramref name="builder"/> with <see cref="DictionaryRetrievalStrategy"/>.
    /// </summary>
    /// <param name="builder">A configuration builder instance for adding <see cref="IConfigurationRepository"/> or it`s descendant.</param>
    /// <param name="repository">An <see cref="IConfigurationRepository"/> object.</param>
    /// <param name="configureSource">If set, configures <see cref="ConfigurationRepositorySource"/>.</param>
    /// <returns>An <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddDictionaryRepository<TSource>(
        this IConfigurationBuilder builder,
        IConfigurationRepository repository,
        Action<TSource>? configureSource = null)
        where TSource : ConfigurationRepositorySource, new()
    {
        var source = new TSource
        {
            Repository = repository,
            RetrievalStrategy = DictionaryRetrievalStrategy.Instance
        };
        configureSource?.Invoke(source);

        builder.Add(source);
        return builder;
    }

    /// <summary>
    /// Adds an <see cref="IConfigurationRepository"/> object to <paramref name="builder"/> with <see cref="ParsableRetrievalStrategy"/>.
    /// </summary>
    /// <param name="builder">A configuration builder instance for adding <see cref="IConfigurationRepository"/> or it`s descendant.</param>
    /// <param name="repository">An <see cref="IConfigurationRepository"/> object.</param>
    /// <param name="parserFactory">A factory method that returns an instance of
    /// configuration parser to be used for parsing data being loaded from repository. If not specified then a factory
    /// creating instance of <see cref="JsonConfigurationParser"/> is used by default.</param>
    /// <param name="configureSource">If set, configures <see cref="ConfigurationRepositorySource"/>.</param>
    /// <returns>An <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddParsableRepository<TSource>(
        this IConfigurationBuilder builder,
        IConfigurationRepository repository,
        Func<IConfigurationParser>? parserFactory = null,
        Action<TSource>? configureSource = null)
        where TSource : ConfigurationRepositorySource, new()
    {
        parserFactory ??= () => new JsonConfigurationParser();

        var source = new TSource
        {
            Repository = repository,
            RetrievalStrategy = new ParsableRetrievalStrategy(parserFactory)
        };
        configureSource?.Invoke(source);

        builder.Add(source);
        return builder;
    }

    /// <summary>
    /// Adds an <see cref="IConfigurationRepository"/> object to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">A configuration builder instance for adding <see cref="IConfigurationRepository"/>.</param>
    /// <param name="repository">An <see cref="IConfigurationRepository"/> object.</param>
    /// <param name="configureSource">If set, configures <see cref="ConfigurationRepositorySource"/>.</param>
    /// <returns>An <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddRepository(
        this IConfigurationBuilder builder,
        IConfigurationRepository repository,
        Action<ConfigurationRepositorySource>? configureSource = null)
    {
        return builder.AddRepository<ConfigurationRepositorySource>(repository, configureSource);
    }

    /// <summary>
    /// Adds an <see cref="IConfigurationRepository"/> object to <paramref name="builder"/> with <see cref="DictionaryRetrievalStrategy"/>.
    /// </summary>
    /// <param name="builder">A configuration builder instance for adding <see cref="IConfigurationRepository"/>.</param>
    /// <param name="repository">An <see cref="IConfigurationRepository"/> object.</param>
    /// <param name="configureSource">If set, configures <see cref="ConfigurationRepositorySource"/>.</param>
    /// <returns>An <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddDictionaryRepository(
        this IConfigurationBuilder builder,
        IConfigurationRepository repository,
        Action<ConfigurationRepositorySource>? configureSource = null)
    {
        return builder.AddDictionaryRepository<ConfigurationRepositorySource>(repository, configureSource);
    }

    /// <summary>
    /// Adds an <see cref="IConfigurationRepository"/> object to <paramref name="builder"/> with <see cref="DictionaryRetrievalStrategy"/>.
    /// </summary>
    /// <param name="builder">A configuration builder instance for adding <see cref="IConfigurationRepository"/>.</param>
    /// <param name="repository">An <see cref="IConfigurationRepository"/> object.</param>
    /// <param name="parserFactory">A factory method that returns an instance of
    /// configuration parser to be used for parsing data being loaded from repository. If not specified then a factory
    /// creating instance of <see cref="JsonConfigurationParser"/> is used by default.</param>
    /// <param name="configureSource">If set, configures <see cref="ConfigurationRepositorySource"/>.</param>
    /// <returns>An <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddParsableRepository(
        this IConfigurationBuilder builder,
        IConfigurationRepository repository,
        Func<IConfigurationParser>? parserFactory = null,
        Action<ConfigurationRepositorySource>? configureSource = null)
    {
        return builder.AddParsableRepository<ConfigurationRepositorySource>(repository, parserFactory, configureSource);
    }

    /// <summary>
    /// Sets a default action to be invoked for repository providers when an error occurs.
    /// </summary>
    /// <param name="builder">A configuration builder instance for adding property with handler.</param>
    /// <param name="handler">An Action to be invoked on a database load exception.</param>
    /// <returns>An <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder SetRepositoryLoadExceptionHandler(this IConfigurationBuilder builder,
        Action<RepositoryLoadExceptionContext> handler)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));

        builder.Properties[RepositoryLoadExceptionHandlerKey] = handler;
        return builder;
    }

    /// <summary>
    /// Gets a default action to be invoked for repository providers when an error occurs.
    /// </summary>
    /// <param name="builder">An <see cref="IConfigurationBuilder"/>.</param>
    /// <returns>An <see cref="Action{RepositoryLoadExceptionContext}"/> to be invoked on a database load exception, if set.</returns>
    public static Action<RepositoryLoadExceptionContext>? GetRepositoryLoadExceptionHandler(this IConfigurationBuilder builder)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));

        return builder.Properties.TryGetValue(RepositoryLoadExceptionHandlerKey, out object? handler)
            ? handler as Action<RepositoryLoadExceptionContext>
            : null;
    }

    /// <summary>
    /// Sets a <see cref="IConfigurationParser"/> factory that returns an instance
    /// of configuration parser to be used for parsing data being loaded from repository.
    /// </summary>
    /// <param name="builder">An <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="parserFactory">A factory method that returns an instance of
    /// configuration parser to be used for parsing data being loaded from repository.</param>
    /// <returns>An <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder SetConfigurationParserFactory(this IConfigurationBuilder builder,
        Func<IConfigurationParser> parserFactory)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));

        builder.Properties[ConfigurationParserFactoryKey] = parserFactory;
        return builder;
    }

    /// <summary>
    /// Gets a <see cref="IConfigurationParser"/> factory from <paramref name="builder"/>
    /// properties. That factory returns an instanceof configuration parser to be used
    /// for parsing data being loaded from repository. If no one is found then a factory
    /// creating instance of <see cref="JsonConfigurationParser"/> is used by default.
    /// </summary>
    /// <param name="builder">An <see cref="IConfigurationBuilder"/>.</param>
    /// <returns>A factory method that returns an instance of
    /// configuration parser to be used for parsing data being loaded from repository.</returns>
    public static Func<IConfigurationParser>? GetConfigurationParserFactory(this IConfigurationBuilder builder)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));

        return (builder.Properties.TryGetValue(ConfigurationParserFactoryKey, out object? parserFactory)
            ? (Func<IConfigurationParser>)parserFactory : null) ?? (() => new JsonConfigurationParser());
    }

    /// <summary>
    /// Sets connection string that will be used to connect to the database.
    /// </summary>
    /// <param name="builder">An <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="connectionString">A connection string to connect to the database.</param>
    /// <returns>An <see cref="IConfigurationBuilder"/>.</returns>
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
    /// <param name="builder">An <see cref="IConfigurationBuilder"/>.</param>
    /// <returns>A connection string to connect to the database.</returns>
    public static string? GetDatabaseConnectionString(this IConfigurationBuilder builder)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));

        return builder.Properties.TryGetValue(RepositoryDatabaseConnectionStringKey, out object? connectionString)
            ? (string)connectionString : null;
    }

    /// <summary>
    /// Gets the <see cref="IConfigurationRepository"/> that will be used to store configurations.
    /// </summary>
    /// <param name="builder">An <see cref="IConfigurationBuilder"/>.</param>
    /// <returns>An <see cref="IConfigurationRepository"/>.</returns>
    public static IConfigurationRepository? GetConfigurationRepository(this IConfigurationBuilder builder)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));

        return (builder.Properties.TryGetValue(RepositoryKey, out object? repository))
            ? (IConfigurationRepository)repository : null;
    }

    private const string RepositoryKey = "ConfigurationRepository:Key";
    private const string RepositoryLoadExceptionHandlerKey = "ConfigurationRepository:LoadExceptionHandler";
    private const string ConfigurationParserFactoryKey = "ConfigurationRepository:ParserFactory";
    private const string RepositoryDatabaseConnectionStringKey = "ConfigurationRepository:DatabaseConnectionString";
}
