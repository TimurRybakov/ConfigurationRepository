using ConfigurationRepository.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace ConfigurationRepository.Tests;

internal sealed class EntryRepository : IUpdatableRepository
{
    private readonly DictionaryRepositoryDbContext _context;

    public EntryRepository(DictionaryRepositoryDbContext context)
    {
        _context = context;
        RetrievalStrategy = DictionaryRetrievalStrategy.Instance;
    }

    public IRetrievalStrategy RetrievalStrategy { get; }

    public Task<List<DictionaryConfigurationEntry>> GetAllAsync() =>
        _context.ConfigurationEntryDbSet.ToListAsync();

    public async Task<DictionaryConfigurationEntry?> GetByIdAsync(string key) =>
        await _context.ConfigurationEntryDbSet.FindAsync(key);

    public async Task AddAsync(DictionaryConfigurationEntry entry)
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
