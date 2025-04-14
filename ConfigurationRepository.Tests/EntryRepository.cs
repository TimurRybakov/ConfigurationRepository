using ConfigurationRepository.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace ConfigurationRepository.Tests;

internal sealed class EntryRepository : IUpdatableRepository
{
    private readonly RepositoryDbContext _context;

    public EntryRepository(RepositoryDbContext context)
    {
        _context = context;
    }

    public Task<List<ConfigurationEntry>> GetAllAsync() =>
        _context.ConfigurationEntryDbSet.ToListAsync();

    public async Task<ConfigurationEntry?> GetByIdAsync(string key) =>
        await _context.ConfigurationEntryDbSet.FindAsync(key);

    public async Task AddAsync(ConfigurationEntry entry)
    {
        _context.ConfigurationEntryDbSet.Add(entry);
        await _context.SaveChangesAsync();
    }

    public bool IsReloadNeeded()
    {
        return true;
    }

    public IDictionary<string, string?> GetConfiguration() =>
        _context.ConfigurationEntryDbSet.AsNoTracking()
        .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.OrdinalIgnoreCase);
}
