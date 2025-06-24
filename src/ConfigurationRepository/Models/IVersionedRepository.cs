namespace ConfigurationRepository;

/// <summary>
/// A version repository interface. Versioned repositories reloads data only on changed versions.
/// </summary>
public interface IVersionedRepository : IConfigurationRepository
{
    /// <summary>
    /// Checks if version was changed and reaload is actually needed.
    /// </summary>
    /// <returns>If true then GetConfiguration() will be made soon after call to this method.</returns>

    bool VersionChanged();
}
