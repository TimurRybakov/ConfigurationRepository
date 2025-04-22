using System.Text;

namespace ConfigurationRepository;

/// <summary>
/// The strategy is used to extract data from a repository represented as a value accessed by a key.
/// The value is a UTF8-string that would be parsed into a configuration dictionary.
/// </summary>
public class ParsableRetrievalStrategy : IRetrievalStrategy
{
    private readonly Func<IConfigurationParser> _parserFactory;

    public ParsableRetrievalStrategy(Func<IConfigurationParser> parserFactory)
    {
        _parserFactory = parserFactory;
    }

    public IDictionary<string, string?> RetrieveConfiguration(IRepository repository)
    {
        var data = repository.GetConfiguration<string>();

        using Stream stream = CreateStream(data);

        var parser = _parserFactory();

        return parser.Parse(stream);
    }

    private static Stream CreateStream(string input)
    {
        byte[] byteArray = Encoding.UTF8.GetBytes(input);

        return new MemoryStream(byteArray);
    }

}
