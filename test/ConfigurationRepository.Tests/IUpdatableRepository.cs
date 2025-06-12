using ConfigurationRepository.EntityFramework;

namespace ConfigurationRepository.Tests;

internal interface IUpdatableRepository : IRepository
{
    Task AddAsync(ConfigurationEntry entry);
}
