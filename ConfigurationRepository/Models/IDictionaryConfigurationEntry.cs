using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace ConfigurationRepository;

/// <summary>
/// A single configuration entry stored in an <see cref="IRepository"/>.
/// </summary>
public interface IDictionaryConfigurationEntry
{
    /// <summary>
    /// Configuration Key delimited by <see cref="ConfigurationPath.KeyDelimiter"/>.
    /// </summary>
    [DisallowNull]
    string Key { get; }

    /// <summary>
    /// Nullable string represeting configuration Value.
    /// </summary>
    string? Value { get; set; }
}
