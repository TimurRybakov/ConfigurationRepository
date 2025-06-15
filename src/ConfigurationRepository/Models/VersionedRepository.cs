using System.Collections;

namespace ConfigurationRepository;

/// <summary>
/// Common logic for versioned repositories.
/// </summary>
public abstract class VersionedRepository : IVersionedRepository
{
    /// <summary>
    /// Current configuration version.
    /// </summary>
    protected byte[]? _version;

    /// <inheritdoc/>
    public abstract TData GetConfiguration<TData>();

    /// <inheritdoc/>
    public bool VersionChanged()
    {
        if (!IsVersioned())
            return true;

        var newVersion = GetCurrentVersion();

        if (StructuralComparisons.StructuralComparer.Compare(newVersion, _version) == 0)
        {
            return false;
        }

        _version = newVersion;
        return true;
    }

    /// <summary>
    /// Checks if current <see cref="VersionedRepository"/> object state defines a versioned repository.
    /// </summary>
    /// <returns>True if repository is versioned.</returns>
    protected abstract bool IsVersioned();

    /// <summary>
    /// Gets configuration current version from database.
    /// </summary>
    /// <returns>An array of bytes.</returns>
    protected abstract byte[]? GetCurrentVersion();

    /// <summary>
    /// Ensures that repository is versioned and version field is initialized with current version from database.
    /// </summary>
    protected void CheckVersionInitialized()
    {
        if (_version is null && IsVersioned())
        {
            _version = GetCurrentVersion();
        }
    }
}
