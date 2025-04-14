using Microsoft.EntityFrameworkCore;

namespace ConfigurationRepository.EntityFramework;

internal sealed class EfConfigurationRepository<TDbContext, TEntry> : IRepository
    where TDbContext : DbContext, IRepositoryDbContext<TEntry>
    where TEntry : class, IEntry
{
    private TDbContext DbContext { get; }

    public EfConfigurationRepository(TDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public IDictionary<string, string?> GetConfiguration() =>
        DbContext.ConfigurationEntryDbSet.AsNoTracking()
        .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.OrdinalIgnoreCase);
}
