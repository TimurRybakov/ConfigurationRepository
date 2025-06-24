using System.Text;

namespace ConfigurationRepository;

/// <summary>
/// Parsable retrieval strategy is used to extract data from a repository.
/// During exstraction data is serialized in single value accessed by a key.
/// The value is a UTF8-string that would be parsed into a configuration dictionary.
/// </summary>
public class ParsableRetrievalStrategy(Func<IConfigurationParser> parserFactory) : IRetrievalStrategy
{
    /// <inheritdoc/>
    public IDictionary<string, string?> RetrieveConfiguration(IConfigurationRepository repository)
    {
        var data = repository.GetConfiguration<string>();

        using Stream stream = CreateStream(data);

        return parserFactory().Parse(stream);
    }

    private static MemoryStream CreateStream(string input)
    {
        byte[] byteArray = Encoding.UTF8.GetBytes(input);

        return new MemoryStream(byteArray);
    }
}
