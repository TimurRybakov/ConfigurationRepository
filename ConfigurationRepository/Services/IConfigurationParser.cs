namespace ConfigurationRepository;

public interface IConfigurationParser
{
    IDictionary<string, string?> Parse(Stream input);
}
