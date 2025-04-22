
using System.Diagnostics.CodeAnalysis;

namespace ConfigurationRepository.SqlClient;

public class SqlClientDictionaryConfigurationSource : ConfigurationRepositorySource
{
    [DisallowNull]
    public string? ConnectionString { get; set; }
}
