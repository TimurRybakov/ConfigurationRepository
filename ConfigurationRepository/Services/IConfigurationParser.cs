namespace ConfigurationRepository;

/// <summary>
/// A parser that is used in <see cref="ParsableRetrievalStrategy"/>.
/// </summary>
public interface IConfigurationParser
{
    /// <summary>
    /// Parses <paramref name="input"/> stream to configuration dictionary.
    /// </summary>
    /// <param name="input">A <see cref="Stream"/> object that provides data.</param>
    /// <returns></returns>
    IDictionary<string, string?> Parse(Stream input);
}
