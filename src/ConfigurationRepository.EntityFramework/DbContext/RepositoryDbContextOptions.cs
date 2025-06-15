using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Text;

namespace ConfigurationRepository.EntityFramework;

/// <summary>
/// Repository database option extensions.
/// </summary>
public class RepositoryDbContextOptions : IDbContextOptionsExtension
{
    /// <summary>
    /// The name for a table that stores configuration as key-value pairs.
    /// </summary>
    public string TableName { get; set; } = null!;

    /// <summary>
    /// The name for a schema of configuration table.
    /// </summary>
    public string? SchemaName { get; set; }

    /// <summary>
    /// The name for a column storing keys.
    /// </summary>
    public string KeyColumnName { get; set; } = "Key";

    /// <summary>
    /// The name for a column storing values.
    /// </summary>
    public string ValueColumnName { get; set; } = "Value";

    /// <inheritdoc/>
    public void ApplyServices(IServiceCollection services)
    {
    }

    /// <inheritdoc/>
    public void Validate(IDbContextOptions options)
    {
        if (string.IsNullOrEmpty(TableName))
        {
            throw new InvalidOperationException("table name is not set");
        }
    }

    /// <inheritdoc/>
    public DbContextOptionsExtensionInfo Info => new ExtensionInfo(this);

    /// <inheritdoc/>
    public RepositoryDbContextOptions() { }

    /// <summary>
    /// Creates a new instance of <see cref="RepositoryDbContextOptions"/> and copies fields values from <paramref name="copyFrom"/>.
    /// </summary>
    /// <param name="copyFrom"></param>
    protected RepositoryDbContextOptions(RepositoryDbContextOptions copyFrom)
    {
        SchemaName = copyFrom.SchemaName;
        TableName = copyFrom.TableName;
        KeyColumnName = copyFrom.KeyColumnName;
        ValueColumnName = copyFrom.ValueColumnName;
    }

    private sealed class ExtensionInfo(IDbContextOptionsExtension extension) : DbContextOptionsExtensionInfo(extension)
    {
        private string? _logFragment;
        private int? _serviceProviderHashCode;

        private new RepositoryDbContextOptions Extension
            => (RepositoryDbContextOptions)base.Extension;

        public override bool IsDatabaseProvider => false;

        public override string LogFragment
        {
            get
            {
                if (_logFragment is null)
                {
                    var builder = new StringBuilder();

                    builder.Append("using db configuration table ");
                    if (Extension.SchemaName != null)
                    {
                        builder.Append($"{Extension.SchemaName}.");
                    }

                    builder.Append($"{Extension.TableName} {{ {Extension.KeyColumnName}, {Extension.ValueColumnName} }}");

                    _logFragment = builder.ToString();
                }

                return _logFragment;
            }
        }

        public override int GetServiceProviderHashCode()
        {
            if (_serviceProviderHashCode is null)
            {
                var hashCode = new HashCode();
                hashCode.Add(Extension.TableName);
                hashCode.Add(Extension.SchemaName);

                _serviceProviderHashCode = hashCode.ToHashCode();
            }

            return _serviceProviderHashCode.Value;
        }

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            => other is ExtensionInfo;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            if (Extension.SchemaName is not null)
            {
                debugInfo["StoredConfiguration.SchemaName"]
                    = Extension.SchemaName.GetHashCode().ToString(CultureInfo.InvariantCulture);
            }

            debugInfo["StoredConfiguration.TableName"]
                = Extension.TableName.GetHashCode().ToString(CultureInfo.InvariantCulture);
        }
    }
}
