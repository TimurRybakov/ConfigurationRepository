namespace ConfigurationRepository;

/// <summary>
/// Extension methods for <see cref="IReloadableConfigurationSource"/>.
/// </summary>
public static class ReloadableConfigurationSourceExtensions
{
    /// <summary>
    /// Sets PeriodicalReload property of <see cref="IReloadableConfigurationSource"/> to true.
    /// This tells the reloadable configuration provider to be reloaded periodically by <see cref="ConfigurationReloader"/> hosted service.
    /// </summary>
    /// <typeparam name="TSource">A type of <see cref="IReloadableConfigurationSource"/>.</typeparam>
    /// <param name="source">Instance of <see cref="IReloadableConfigurationSource"/> type or it`s descendant.</param>
    /// <returns>An instance of <see cref="IReloadableConfigurationSource"/> type.</returns>
    public static TSource WithPeriodicalReload<TSource>(this TSource source)
        where TSource : IReloadableConfigurationSource
    {
        source.PeriodicalReload = true;
        return source;
    }
}
