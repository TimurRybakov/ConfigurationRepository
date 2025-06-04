using System;
using System.Collections.Generic;
using System.Linq;

namespace ConfigurationRepository;

public static class PeriodicalReloadExtensions
{

    public static TSource WithPeriodicalReload<TSource>(this TSource source)
        where TSource : IReloadableConfigurationSource
    {
        source.PeriodicalReload = true;
        return source;
    }
}
