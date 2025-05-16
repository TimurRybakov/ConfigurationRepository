
namespace ParametrizedConfiguration;

[Serializable]
public class CyclicDependencyException : Exception
{
    public CyclicDependencyException()
    {
    }

    public CyclicDependencyException(string? message) : base(message)
    {
    }

    public CyclicDependencyException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
