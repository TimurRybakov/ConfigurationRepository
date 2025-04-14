using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository;

/// <summary>
/// A storage for <see cref="IConfigurationProvider"/> to access a configuration.
/// </summary>
public interface IRepository
{
    /// <summary>
    /// Checks if reaload actually needed, i.e. stored configuration changed. Defaults to true.
    /// </summary>
    /// <returns>If true then GetConfiguration() will be made soon after call to this method.</returns>
    bool IsReloadNeeded() => true;

    /// <summary>
    /// Get configuration entries.
    /// </summary>
    IDictionary<string, string?> GetConfiguration();
}
