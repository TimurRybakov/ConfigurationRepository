namespace ConfigurationRepository;

/// <summary>
/// Strategy that will be used to retrieve configuration from repository.
/// </summary>
public interface IRetrievalStrategy
{
    /// <summary>
    /// A retrieval strategy method that retuns configuration as dictionary from <paramref name="repository"/>.
    /// </summary>
    /// <param name="repository">A repository that stores configuration.</param>
    /// <returns>Configuration as dictionary.</returns>
    IDictionary<string, string?> RetrieveConfiguration(IConfigurationRepository repository);
}
