using ConfigurationRepository.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace ConfigurationRepository.IntegrationTests;

internal sealed class EntryRepository(RepositoryDbContext context) : IUpdatableRepository
{
    public IRetrievalStrategy RetrievalStrategy { get; } = DictionaryRetrievalStrategy.Instance;

    public Task<List<ConfigurationEntry>> GetAllAsync() =>
        context.ConfigurationEntryDbSet.ToListAsync();

    public async Task<ConfigurationEntry?> GetByIdAsync(string key) =>
        await context.ConfigurationEntryDbSet.FindAsync(key);

    public async Task AddAsync(ConfigurationEntry entry)
    {
        context.ConfigurationEntryDbSet.Add(entry);
        await context.SaveChangesAsync();
    }

    public TData GetConfiguration<TData>() => (TData)(IDictionary<string, string?>)GetConfiguration();

    private Dictionary<string, string?> GetConfiguration()
    {
        return context.ConfigurationEntryDbSet.AsNoTracking()
            .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.OrdinalIgnoreCase);
    }

    public static bool VersionIsChanged()
    {
        return true;
    }
}
