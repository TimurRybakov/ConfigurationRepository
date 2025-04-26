using Microsoft.EntityFrameworkCore;

namespace ConfigurationRepository.EntityFramework;

public interface IRepositoryDbContext<TEntry>
    where TEntry : class, IConfigurationEntry
{
    DbSet<TEntry> ConfigurationEntryDbSet { get; }
}
