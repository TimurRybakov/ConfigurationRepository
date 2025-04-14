namespace ConfigurationRepository;

/// <summary>
/// Contains information about a database load exception.
/// </summary>
public class RepositoryLoadExceptionContext
{
    /// <summary>
    /// The <see cref="ConfigurationRepositoryProvider"/> that caused the exception.
    /// </summary>
    public ConfigurationRepositoryProvider Provider { get; set; } = null!;

    /// <summary>
    /// The exception that occurred in Load.
    /// </summary>
    public Exception Exception { get; set; } = null!;

    /// <summary>
    /// If true, the exception will not be rethrown.
    /// </summary>
    public bool Ignore { get; set; }
}
