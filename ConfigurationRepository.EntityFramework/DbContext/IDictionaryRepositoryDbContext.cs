using Microsoft.EntityFrameworkCore;

namespace ConfigurationRepository.EntityFramework;

public interface IDictionaryRepositoryDbContext<TEntry>
    where TEntry : class, IDictionaryConfigurationEntry
{
    DbSet<TEntry> ConfigurationEntryDbSet { get; }
}
