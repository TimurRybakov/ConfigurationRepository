using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository;

/// <summary>
/// A single configuration entry stored in an <see cref="IConfigurationRepository"/>.
/// </summary>
/// <param name="key">A configuration key.</param>
/// <param name="value">A configuration value.</param>
[method: SetsRequiredMembers]
public sealed class ConfigurationEntry(string key, string? value) : IConfigurationEntry
{
    /// <summary>
    /// Configuration Key delimited by <see cref="ConfigurationPath.KeyDelimiter"/>.
    /// </summary>
    [DisallowNull]
    public required string Key { get; init; } = key;

    /// <summary>
    /// Nullable string represeting configuration Value.
    /// </summary>
    public string? Value { get; set; } = value;
}
