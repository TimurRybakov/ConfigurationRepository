namespace ConfigurationRepository.IntegrationTests;

internal interface IUpdatableRepository : IRepository
{
    Task AddAsync(ConfigurationEntry entry);
}
