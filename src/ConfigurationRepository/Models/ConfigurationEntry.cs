using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository;

/// <summary>
/// A single configuration entry stored in an <see cref="IRepository"/>.
/// </summary>
public class ConfigurationEntry : IConfigurationEntry
{
    /// <summary>
    /// Configuration Key delimited by <see cref="ConfigurationPath.KeyDelimiter"/>.
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Nullable string represeting configuration Value.
    /// </summary>
    public string? Value { get; set; }
}
