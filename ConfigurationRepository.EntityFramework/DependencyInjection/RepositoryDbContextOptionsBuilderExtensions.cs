
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ConfigurationRepository.EntityFramework;

public static class RepositoryDbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder<TContext> UseTable<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        string tableName,
        string? schemaName = null,
        Action<DictionaryRepositoryDbContextOptions>? configurator = null)
        where TContext : DictionaryRepositoryDbContext
    {
        var extension = (optionsBuilder.Options.FindExtension<DictionaryRepositoryDbContextOptions>()
                ?? new DictionaryRepositoryDbContextOptions());

        extension.TableName = tableName;

        if (schemaName is not null)
        {
            extension.SchemaName = schemaName;
        }

        configurator?.Invoke(extension);

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }
}
