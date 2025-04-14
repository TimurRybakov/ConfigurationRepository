namespace ConfigurationRepository;

public class RepositoryLoadException(string? message = null)
    : Exception(message ?? "Error loading configuration from repository.");
