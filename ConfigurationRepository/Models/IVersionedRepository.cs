namespace ConfigurationRepository;

public interface IVersionedRepository : IRepository
{
    /// <summary>
    /// Checks if version was changed and reaload is actually needed.
    /// </summary>
    /// <returns>If true then GetConfiguration() will be made soon after call to this method.</returns>

    bool VersionChanged();
}
