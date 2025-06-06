using Microsoft.EntityFrameworkCore;

namespace ConfigurationRepository.EntityFramework;

/// <summary>
/// Configuration repository database context abstraction.
/// </summary>
/// <typeparam name="TEntry"></typeparam>
public interface IRepositoryDbContext<TEntry>
    where TEntry : class, IConfigurationEntry
{
    DbSet<TEntry> ConfigurationEntryDbSet { get; }
}
