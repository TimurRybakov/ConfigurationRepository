
using Microsoft.Extensions.Primitives;

namespace ConfigurationRepository.Services;

public class RepositoryChangesNotifier : IRepositoryChangesNotifier
{
    private readonly Func<CancellationTokenSource> _createCancellationTokenSource;
    private volatile CancellationTokenSource? _cancellationTokenSource;

    public RepositoryChangesNotifier()
    {
        _createCancellationTokenSource = () => new CancellationTokenSource();
    }

    public RepositoryChangesNotifier(TimeSpan reloadPeriod)
    {
        _createCancellationTokenSource = () => new CancellationTokenSource(reloadPeriod);
    }

    public IChangeToken CreateChangeToken()
    {
        var previousToken = Interlocked.Exchange(
            ref _cancellationTokenSource, _createCancellationTokenSource());
        previousToken?.Dispose();
        return new CancellationChangeToken(_cancellationTokenSource!.Token);
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
    }

    public void DoReload()
    {
        _cancellationTokenSource?.Cancel();
    }
}
