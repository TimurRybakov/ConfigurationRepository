
using ConfigurationRepository.Services;

namespace ConfigurationRepository;

public static class ConfigurationRepositorySourceExtensions
{
    public static ConfigurationRepositorySource UseOptional(this ConfigurationRepositorySource source)
    {
        source.Optional = true;
        return source;
    }

    public static ConfigurationRepositorySource WithPeriodicalReload(this ConfigurationRepositorySource source)
    {
        source.PeriodicalReload = true;
        return source;
    }

    public static T UseRepositoryChangesNotifier<T>(this T source, IRepositoryChangesNotifier repositoryChangesNotifier)
        where T : ConfigurationRepositorySource
    {
        source.RepositoryChangesNotifier = repositoryChangesNotifier;

        return source;
    }

    public static T UseRepositoryChangesNotifier<T>(this T source, TimeSpan? reloadPeriod = null)
        where T : ConfigurationRepositorySource
    {
        var repositoryChangesNotifier = reloadPeriod is null
            ? new RepositoryChangesNotifier()
            : new RepositoryChangesNotifier(reloadPeriod.Value);

        source.UseRepositoryChangesNotifier(repositoryChangesNotifier);

        return source;
    }
}
