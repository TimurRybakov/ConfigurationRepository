using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository;

/// <summary>
/// Source that configures <see cref="ConfigurationRepositoryProvider"/>.
/// </summary>
public class ConfigurationRepositorySource : IReloadableConfigurationSource
{
    /// <summary>
    /// The configuration storage repository.
    /// </summary>
    [DisallowNull]
    public IConfigurationRepository? Repository { get; set; }

    /// <summary>
    /// Strategy that will be used to retrieve configuration from repository.
    /// <see cref="DictionaryRetrievalStrategy"/> is used by default if not set.
    /// </summary>
    [DisallowNull]
    public IRetrievalStrategy? RetrievalStrategy { get; set; }

    /// <summary>
    /// Determines if loading the configuration is optional.
    /// </summary>
    public bool Optional { get; set; } = false;

    /// <summary>
    /// True means that configuration provider will be reloaded periodically by <see cref="ConfigurationReloader"/> service
    /// </summary>
    public bool PeriodicalReload { get; set; } = false;

    /// <summary>
    /// A service used to track configuration repository changes using change tokens.
    /// </summary>
    public IRepositoryChangesNotifier? RepositoryChangesNotifier { get; set; }

    /// <summary>
    /// Will be called if an uncaught exception occurs in ConfigurationRepositoryProvider.Load() method.
    /// </summary>
    public Action<RepositoryLoadExceptionContext>? OnLoadException { get; set; }

    /// <summary>
    /// Builds the <see cref="IConfigurationProvider"/> for this source.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
    /// <returns>A <see cref="IConfigurationProvider"/></returns>
    public virtual IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        Repository ??= builder.GetConfigurationRepository() ?? throw new NullReferenceException($"{nameof(Repository)} is not set.");
        RetrievalStrategy ??= DictionaryRetrievalStrategy.Instance;

        return new ConfigurationRepositoryProvider(this);
    }
}
