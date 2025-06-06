using Microsoft.EntityFrameworkCore;

namespace ConfigurationRepository.EntityFramework;

/// <summary>
/// A dictionary repository that uses Ef Core to fetch data from database.
/// </summary>
internal sealed class EfDictionaryConfigurationRepository<TDbContext, TEntry> : IRepository
    where TDbContext : DbContext, IRepositoryDbContext<TEntry>
    where TEntry : class, IConfigurationEntry
{
    private TDbContext DbContext { get; }

    public EfDictionaryConfigurationRepository(TDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public TData GetConfiguration<TData>()
    {
        return (TData)GetConfiguration();
    }

    private IDictionary<string, string?> GetConfiguration() =>
        DbContext.ConfigurationEntryDbSet.AsNoTracking()
        .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.OrdinalIgnoreCase);
}
