
using Microsoft.Extensions.Primitives;

namespace ConfigurationRepository;

public interface IRepositoryChangesNotifier : IDisposable
{
    /// <summary>
    /// Tells the provider to reload the data
    /// </summary>
    void DoReload();

    /// <summary>
    /// Creates new <see cref="IChangeToken"/>
    /// </summary>
    IChangeToken CreateChangeToken();
}
