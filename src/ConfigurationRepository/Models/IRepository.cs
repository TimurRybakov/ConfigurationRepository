using Microsoft.Extensions.Configuration;

namespace ConfigurationRepository;

/// <summary>
/// A storage for <see cref="IConfigurationProvider"/> to access a configuration.
/// </summary>
public interface IRepository
{
    /// <summary>
    /// Gets configuration of type TData.
    /// </summary>
    /// <typeparam name="TData">Type of data to be returned by this method.</typeparam>
    /// <returns></returns>
    TData GetConfiguration<TData>();
}
