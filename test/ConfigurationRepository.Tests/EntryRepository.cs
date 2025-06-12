using ConfigurationRepository.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace ConfigurationRepository.Tests;

internal sealed class EntryRepository : IUpdatableRepository
{
    private readonly RepositoryDbContext _context;

    public EntryRepository(RepositoryDbContext context)
    {
        _context = context;
        RetrievalStrategy = DictionaryRetrievalStrategy.Instance;
    }

    public IRetrievalStrategy RetrievalStrategy { get; }

    public Task<List<ConfigurationEntry>> GetAllAsync() =>
        _context.ConfigurationEntryDbSet.ToListAsync();

    public async Task<ConfigurationEntry?> GetByIdAsync(string key) =>
        await _context.ConfigurationEntryDbSet.FindAsync(key);

    public async Task AddAsync(ConfigurationEntry entry)
    {
        _context.ConfigurationEntryDbSet.Add(entry);
        await _context.SaveChangesAsync();
    }

    public bool VersionIsChanged()
    {
        return true;
    }

    public TData GetConfiguration<TData>()
    {
        return (TData)GetConfiguration();
    }

    private IDictionary<string, string?> GetConfiguration()
    {
        return _context.ConfigurationEntryDbSet.AsNoTracking()
            .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.OrdinalIgnoreCase);
    }
}
