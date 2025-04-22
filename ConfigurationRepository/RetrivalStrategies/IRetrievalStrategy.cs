namespace ConfigurationRepository;

/// <summary>
/// Strategy that will be used to retrieve configuration from repository.
/// </summary>
public interface IRetrievalStrategy
{
    IDictionary<string, string?> RetrieveConfiguration(IRepository repository);
}
