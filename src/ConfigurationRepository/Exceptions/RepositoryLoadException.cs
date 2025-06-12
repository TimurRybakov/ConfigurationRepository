namespace ConfigurationRepository;

/// <summary>
/// Occurs when repository fails to load it`s data.
/// </summary>
/// <param name="message">Error message.</param>
public class RepositoryLoadException(string? message = null)
    : Exception(message ?? "Error loading configuration from repository.");
