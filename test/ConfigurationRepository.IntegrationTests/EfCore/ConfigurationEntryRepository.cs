using ConfigurationRepository.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace ConfigurationRepository.IntegrationTests;

internal sealed class ConfigurationEntryRepository(ConfigurationRepositoryDbContext context)
{
    public Task<List<ConfigurationEntry>> GetAllAsync() =>
        context.ConfigurationEntryDbSet.ToListAsync();

    public async Task<ConfigurationEntry?> GetByIdAsync(string key) =>
        await context.ConfigurationEntryDbSet.FindAsync(key);

    public async Task AddAsync(ConfigurationEntry entry)
    {
        context.ConfigurationEntryDbSet.Add(entry);
        await context.SaveChangesAsync();
    }
}
