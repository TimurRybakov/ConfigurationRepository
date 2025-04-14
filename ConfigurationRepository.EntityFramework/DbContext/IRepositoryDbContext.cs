using Microsoft.EntityFrameworkCore;

namespace ConfigurationRepository.EntityFramework;

public interface IRepositoryDbContext<TEntry>
    where TEntry : class, IEntry
{
    DbSet<TEntry> ConfigurationEntryDbSet { get; }
}