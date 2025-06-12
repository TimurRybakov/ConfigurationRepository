
using Microsoft.Extensions.Primitives;

namespace ConfigurationRepository;

/// <summary>
/// An abstraction for service that can be used to track repository
/// configuration changes and to trigger its reload using change tockens.
/// </summary>
public interface IRepositoryChangesNotifier : IDisposable
{
    /// <summary>
    /// Tells the provider to reload the data.
    /// </summary>
    void DoReload();

    /// <summary>
    /// Creates new <see cref="IChangeToken"/>.
    /// </summary>
    IChangeToken CreateChangeToken();
}
