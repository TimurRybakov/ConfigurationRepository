namespace ConfigurationRepository;

/// <summary>
/// A single configuration entry stored in an <see cref="IRepository"/>.
/// </summary>
public class ConfigurationEntry : IConfigurationEntry
{
    public required string Key { get; set; }

    public string? Value { get; set; }
}
