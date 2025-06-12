using System;
using System.Collections.Generic;
using System.Linq;

namespace ConfigurationRepository;

public static class PeriodicalReloadExtensions
{
    /// <summary>
    /// Sets PeriodicalReload property of <see cref="IReloadableConfigurationSource"/> to true.
    /// This tell the reloadable configuration provider to be periodically reloaded by <see cref="ConfigurationReloader"/> hosted service.
    /// </summary>
    /// <typeparam name="TSource">A type of <see cref="IReloadableConfigurationSource"/>.</typeparam>
    /// <param name="source">Instance of <see cref="TSource"/> type.</param>
    /// <returns>An instance of <see cref="IReloadableConfigurationSource"/> type.</returns>
    public static TSource WithPeriodicalReload<TSource>(this TSource source)
        where TSource : IReloadableConfigurationSource
    {
        source.PeriodicalReload = true;
        return source;
    }
}
