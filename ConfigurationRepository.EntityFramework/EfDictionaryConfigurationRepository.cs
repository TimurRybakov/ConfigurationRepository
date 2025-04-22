using Microsoft.EntityFrameworkCore;

namespace ConfigurationRepository.EntityFramework;

internal sealed class EfDictionaryConfigurationRepository<TDbContext, TEntry> : IRepository
    where TDbContext : DbContext, IDictionaryRepositoryDbContext<TEntry>
    where TEntry : class, IDictionaryConfigurationEntry
{
    private TDbContext DbContext { get; }

    public IRetrievalStrategy RetrievalStrategy { get; }

    public EfDictionaryConfigurationRepository(TDbContext dbContext)
    {
        DbContext = dbContext;
        RetrievalStrategy = DictionaryRetrievalStrategy.Instance;
    }

    public TData GetConfiguration<TData>()
    {
        return (TData)GetConfiguration();
    }

    private IDictionary<string, string?> GetConfiguration() =>
        DbContext.ConfigurationEntryDbSet.AsNoTracking()
        .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.OrdinalIgnoreCase);
}
