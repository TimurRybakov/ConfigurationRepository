
using Microsoft.Extensions.Primitives;

namespace ConfigurationRepository;

/// <summary>
/// A service that can be used to track repository configuration changes and to trigger its reload using change tockens.
/// </summary>
public class RepositoryChangesNotifier : IRepositoryChangesNotifier
{
    private readonly Func<CancellationTokenSource> _createCancellationTokenSource;
    private volatile CancellationTokenSource? _cancellationTokenSource;

    /// <inheritdoc/>
    public RepositoryChangesNotifier()
    {
        _createCancellationTokenSource = () => new CancellationTokenSource();
    }

    /// <inheritdoc/>
    public RepositoryChangesNotifier(TimeSpan reloadPeriod)
    {
        _createCancellationTokenSource = () => new CancellationTokenSource(reloadPeriod);
    }

    /// <inheritdoc/>
    public IChangeToken CreateChangeToken()
    {
        var previousToken = Interlocked.Exchange(
            ref _cancellationTokenSource, _createCancellationTokenSource());
        previousToken?.Dispose();
        return new CancellationChangeToken(_cancellationTokenSource!.Token);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public void DoReload()
    {
        _cancellationTokenSource?.Cancel();
    }
}
