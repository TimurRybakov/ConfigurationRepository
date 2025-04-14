
using System.Diagnostics.CodeAnalysis;

namespace ConfigurationRepository.SqlClient;

public class SqlClientConfigurationSource : ConfigurationRepositorySource
{
    [DisallowNull]
    public string? ConnectionString { get; set; }
}
