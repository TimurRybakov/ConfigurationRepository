namespace ConfigurationRepository;

public class ConfigurationEntry : IConfigurationEntry
{
    public required string Key { get; set; }

    public string? Value { get; set; }
}
