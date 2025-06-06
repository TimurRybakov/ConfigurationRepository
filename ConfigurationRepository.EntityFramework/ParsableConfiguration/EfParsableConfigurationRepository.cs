using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace ConfigurationRepository.EntityFramework;

/// <summary>
/// A parsable repository that uses Ef Core to fetch data from database.
/// </summary>
internal sealed class EfParsableConfigurationRepository<TDbContext, TEntry> : IRepository
    where TDbContext : DbContext, IRepositoryDbContext<TEntry>
    where TEntry : class, IConfigurationEntry
{
    private TDbContext DbContext { get; }

    public EfParsableConfigurationRepository(TDbContext dbContext)
    {
        DbContext = dbContext;
    }

    /// <summary>
    /// The key value that identifies desired configuration record.
    /// </summary>
    [DisallowNull]
    public string? Key { get; set; }

    public TData GetConfiguration<TData>()
    {
        return (TData)Convert.ChangeType(GetConfiguration(), typeof(TData));
    }

    private string GetConfiguration()
    {
        var key = Key ?? throw new NullReferenceException($"{nameof(Key)} does not set.");

        return DbContext.ConfigurationEntryDbSet.AsNoTracking().FirstOrDefault(x => x.Key.Equals(key))?.Value
            ?? throw new InvalidOperationException("Null configuration returned from server.");
    }
}
