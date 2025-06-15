
namespace ParametrizedConfiguration;

/// <summary>
/// Represents a cyclic dependency error during configuration parametrization.
/// </summary>
[Serializable]
public class CyclicDependencyException : Exception
{
    /// <inheritdoc/>
    public CyclicDependencyException()
    {
    }

    /// <inheritdoc/>
    public CyclicDependencyException(string? message) : base(message)
    {
    }

    /// <inheritdoc/>
    public CyclicDependencyException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
