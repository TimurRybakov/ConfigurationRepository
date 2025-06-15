using System.Runtime.ExceptionServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace ConfigurationRepository;

/// <summary>
/// A <see cref="ConfigurationProvider"/> that uses repository to access it`s configuration.
/// </summary>
public class ConfigurationRepositoryProvider : ConfigurationProvider, IReloadableConfigurationProvider
{
    private readonly IDisposable? _changeTokenRegistration;
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance with the specified source.
    /// </summary>
    /// <param name="source">The source settings.</param>
    public ConfigurationRepositoryProvider(ConfigurationRepositorySource source)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        _ = Source.Repository ?? throw new NullReferenceException(nameof(Source.Repository));
        _ = Source.RetrievalStrategy ?? throw new NullReferenceException(nameof(Source.RetrievalStrategy));

        if (Source.RepositoryChangesNotifier is not null)
        {
            _changeTokenRegistration = ChangeToken.OnChange(
                () => Source.RepositoryChangesNotifier.CreateChangeToken(),
                Reload);
        }
    }

    /// <summary>
    /// The source settings for this provider.
    /// </summary>
    protected ConfigurationRepositorySource Source { get; }

    /// <summary>
    /// True means that configuration provider will be reloaded periodically by <see cref="ConfigurationReloader"/> service
    /// </summary>
    public bool PeriodicalReload
    {
        get => Source.PeriodicalReload;
        set => Source.PeriodicalReload = value;
    }

    /// <summary>
    /// Generates a string representing this provider name and relevant details.
    /// </summary>
    /// <returns> The configuration name. </returns>
    public override string ToString()
        => $"{GetType().Name} for storage of type {Source.Repository?.GetType().Name ?? "<null>"} ({(Source.Optional ? "Optional" : "Required")})";

    /// <summary>
    /// Loads the configuration from repository/>.
    /// </summary>
    /// <exception cref="RepositoryLoadException">Optional is <c>false</c> on the source and a
    /// configuration cannot be found at the specified Key.</exception>
    /// <exception cref="InvalidDataException">An exception was thrown by the concrete implementation of the
    /// <see cref="Load()"/> method. Use the source <see cref="ConfigurationRepositorySource.OnLoadException"/> callback
    /// if you need more control over the exception.</exception>
    public override void Load()
    {
        Load(reload: false);
    }

    /// <summary>
    /// An inplementation of Reload() method of <see cref="IReloadableConfigurationProvider"/>.
    /// </summary>
    public void Reload()
    {
        Load(reload: true);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose the provider.
    /// </summary>
    /// <param name="disposing"><c>true</c> if invoked from <see cref="IDisposable.Dispose"/>.</param>
    protected virtual void Dispose(bool disposing)
    {
        _changeTokenRegistration?.Dispose();
    }

    /// <inheritdoc/>
    ~ConfigurationRepositoryProvider()
    {
        Dispose(false);
    }

    private void Load(bool reload)
    {
        try
        {
            if (Monitor.TryEnter(_lock))
            {
                var repository = Source.Repository!;

                if (reload
                    && repository is IVersionedRepository versionedRepository
                    && !versionedRepository.VersionChanged())
                    return;

                var configuration = Source.RetrievalStrategy!.RetrieveConfiguration(repository);

                if (configuration.Count == 0)
                {
                    if (Source.Optional)
                        return;

                    throw new RepositoryLoadException($"Stored configuration is empty.");
                }

                Data = configuration;

                OnReload();
            }
        }
        catch (Exception ex)
        {
            if (reload)
            {
                Data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            }

            var exception = new InvalidDataException("Failed to load configuration from repository", ex);
            HandleException(ExceptionDispatchInfo.Capture(exception));
        }
        finally
        {
            if (Monitor.IsEntered(_lock))
            {
                Monitor.Exit(_lock);
            }
        }
    }

    private void HandleException(ExceptionDispatchInfo info)
    {
        bool ignoreException = false;
        if (Source.OnLoadException != null)
        {
            var exceptionContext =
                new RepositoryLoadExceptionContext { Provider = this, Exception = info.SourceException };
            Source.OnLoadException.Invoke(exceptionContext);
            ignoreException = exceptionContext.Ignore;
        }

        if (!ignoreException)
        {
            info.Throw();
        }
    }
}
