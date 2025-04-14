
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ConfigurationRepository.EntityFramework;

public static class RepositoryDbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder UseTable(
        this DbContextOptionsBuilder optionsBuilder,
        string tableName,
        string? schemaName = null,
        Action<RepositoryDbContextOptions>? configurator = null)
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
