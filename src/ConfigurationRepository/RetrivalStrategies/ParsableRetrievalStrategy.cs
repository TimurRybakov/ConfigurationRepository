using System.Text;

namespace ConfigurationRepository;

/// <summary>
/// Parsable retrieval strategy is used to extract data from a repository serialized in single value accessed by a key.
/// The value is a UTF8-string that would be parsed into a configuration dictionary.
/// </summary>
/// <inheritdoc/>
public class ParsableRetrievalStrategy(IConfigurationParser parser) : IRetrievalStrategy
{

    /// <inheritdoc/>
    public IDictionary<string, string?> RetrieveConfiguration(IRepository repository)
    {
        var data = repository.GetConfiguration<string>();

        using Stream stream = CreateStream(data);

        return parser.Parse(stream);
    }

    private static MemoryStream CreateStream(string input)
    {
        byte[] byteArray = Encoding.UTF8.GetBytes(input);

        return new MemoryStream(byteArray);
    }

}
