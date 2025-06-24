namespace ConfigurationRepository.IntegrationTests;

internal interface IUpdatableRepository : IConfigurationRepository
{
    Task AddAsync(ConfigurationEntry entry);
}
