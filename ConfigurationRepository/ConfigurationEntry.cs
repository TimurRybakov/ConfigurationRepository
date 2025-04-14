namespace ConfigurationRepository;

public class ConfigurationEntry : IEntry
{
    public string Key { get; set; } = null!;

    public string? Value { get; set; }
}
