
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ConfigurationRepository.EntityFramework;

/// <summary>
/// Extension methods for <see cref="DbContextOptionsBuilder{TContext}"/>.
/// </summary>
public static class RepositoryDbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Configures table for <see cref="DbContextOptionsBuilder{TContext}"/>.
    /// </summary>
    /// <typeparam name="TContext">Type of <see cref="RepositoryDbContext"/> or it`s descendant.</typeparam>
    /// <param name="optionsBuilder">Database context options builder.</param>
    /// <param name="tableName">Configuration table name in the database.</param>
    /// <param name="schemaName">Schema name of the configuration table.</param>
    /// <param name="configurator">Optional database context options configurator.</param>
    /// <returns>Database context options builder.</returns>
    public static DbContextOptionsBuilder<TContext> UseTable<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        string tableName,
        string? schemaName = null,
        Action<RepositoryDbContextOptions>? configurator = null)
        where TContext : RepositoryDbContext
    {
        var extension = (optionsBuilder.Options.FindExtension<RepositoryDbContextOptions>()
                ?? new RepositoryDbContextOptions());

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
