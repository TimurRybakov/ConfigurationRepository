namespace ConfigurationRepository;

/// <summary>
/// The strategy is used to extract data from a repository represented as a dictionary
/// of key-value pairs.
/// </summary>
public class DictionaryRetrievalStrategy : IRetrievalStrategy
{
    public static readonly DictionaryRetrievalStrategy Instance = new ();

    public IDictionary<string, string?> RetrieveConfiguration(IRepository repository)
    {
        return repository.GetConfiguration<IDictionary<string, string?>>();
    }
}
