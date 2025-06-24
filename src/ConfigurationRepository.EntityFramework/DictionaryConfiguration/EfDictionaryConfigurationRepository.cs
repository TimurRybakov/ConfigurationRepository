using Microsoft.EntityFrameworkCore;

namespace ConfigurationRepository.EntityFramework;

/// <summary>
/// A dictionary repository that uses Ef Core to fetch data from database.
/// Configuration is stored in key-value pairs.
/// </summary>
internal sealed class EfDictionaryConfigurationRepository<TDbContext, TEntry>(TDbContext dbContext) : IConfigurationRepository
    where TDbContext : DbContext, IConfigurationRepositoryDbContext<TEntry>
    where TEntry : class, IConfigurationEntry
{
    public TData GetConfiguration<TData>() => (TData)(IDictionary<string, string?>)GetConfiguration();

    private Dictionary<string, string?> GetConfiguration() =>
        dbContext.ConfigurationEntryDbSet.AsNoTracking()
        .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.OrdinalIgnoreCase);
}
