
namespace ConfigurationRepository;

public static class ConfigurationRepositorySourceExtensions
{
    public static TSource UseOptional<TSource>(this TSource source)
        where TSource : ConfigurationRepositorySource
    {
        source.Optional = true;
        return source;
    }

    public static TSource WithPeriodicalReload<TSource>(this TSource source)
        where TSource : ConfigurationRepositorySource
    {
        source.PeriodicalReload = true;
        return source;
    }

    public static TSource UseRepositoryChangesNotifier<TSource>(
        this TSource source, IRepositoryChangesNotifier repositoryChangesNotifier)
        where TSource : ConfigurationRepositorySource
    {
        source.RepositoryChangesNotifier = repositoryChangesNotifier;

        return source;
    }

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

    public static TSource UseJsonParser<TSource>(this TSource source)
        where TSource : ParsableConfigurationRepositorySource
    {
        source.ConfigurationParser = new JsonConfigurationParser();
        return source;
    }
}
