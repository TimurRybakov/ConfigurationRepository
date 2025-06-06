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
    public string TableName { get; set; } = null!;

    public string? SchemaName { get; set; }

    public string KeyColumnName { get; set; } = "Key";

    public string ValueColumnName { get; set; } = "Value";

    public void ApplyServices(IServiceCollection services)
    {
    }

    public void Validate(IDbContextOptions options)
    {
        if (string.IsNullOrEmpty(TableName))
        {
            throw new InvalidOperationException("table name is not set");
        }
    }

    public DbContextOptionsExtensionInfo Info => new ExtensionInfo(this);

    public RepositoryDbContextOptions() { }

    protected RepositoryDbContextOptions(RepositoryDbContextOptions copyFrom)
    {
        SchemaName = copyFrom.SchemaName;
        TableName = copyFrom.TableName;
        KeyColumnName = copyFrom.KeyColumnName;
        ValueColumnName = copyFrom.ValueColumnName;
    }

    private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
    {
        private string? _logFragment;
        private int? _serviceProviderHashCode;

        public ExtensionInfo(IDbContextOptionsExtension extension) : base(extension) { }

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
