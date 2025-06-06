
namespace ConfigurationRepository;

public static class ConfigurationRepositorySourceExtensions
{
    /// <summary>
    /// Sets Optional property of <see cref="ConfigurationRepositorySource"/>.
    /// An optional provider may contain no data in it`s repository.
    /// </summary>
    /// <typeparam name="TSource">A type of <see cref="ConfigurationRepositorySource"/>.</typeparam>
    /// <param name="source">Instance of <see cref="TSource"/> type.</param>
    /// <returns>An instance of <see cref="ConfigurationRepositorySource"/> type.</returns>
    public static TSource UseOptional<TSource>(this TSource source)
        where TSource : ConfigurationRepositorySource
    {
        source.Optional = true;
        return source;
    }

    /// <summary>
    /// Sets RepositoryChangesNotifier property of <see cref="ConfigurationRepositorySource"/>.
    /// This is a service that used to track configuration repository changes using change tokens.
    /// </summary>
    /// <typeparam name="TSource">A type of <see cref="ConfigurationRepositorySource"/> or it`s descendant.</typeparam>
    /// <param name="source">Instance of <see cref="TSource"/> type.</param>
    /// <param name="repositoryChangesNotifier">Concrete notifier/</param>
    /// <returns>An instance of <see cref="ConfigurationRepositorySource"/> type.</returns>
    public static TSource UseRepositoryChangesNotifier<TSource>(
        this TSource source, IRepositoryChangesNotifier repositoryChangesNotifier)
        where TSource : ConfigurationRepositorySource
    {
        source.RepositoryChangesNotifier = repositoryChangesNotifier;

        return source;
    }

    /// <summary>
    /// Sets RepositoryChangesNotifier property of <see cref="ConfigurationRepositorySource"/>.
    /// This is a service that used to track configuration repository changes using change tokens.
    /// </summary>
    /// <typeparam name="TSource">A type of <see cref="ConfigurationRepositorySource"/> or it`s descendant.</typeparam>
    /// <param name="source">Instance of <see cref="TSource"/> type.</param>
    /// <param name="reloadPeriod">A span of time to wait before reloads.</param>
    /// <returns>An instance of <see cref="ConfigurationRepositorySource"/> type.</returns>
    public static TSource UseRepositoryChangesNotifier<TSource>(
        this TSource source, TimeSpan? reloadPeriod = null)
        where TSource : ConfigurationRepositorySource
    {
        var repositoryChangesNotifier = reloadPeriod is null
            ? new RepositoryChangesNotifier()
            : new RepositoryChangesNotifier(reloadPeriod.Value);

        source.UseRepositoryChangesNotifier(repositoryChangesNotifier);

        return source;
    }
}
