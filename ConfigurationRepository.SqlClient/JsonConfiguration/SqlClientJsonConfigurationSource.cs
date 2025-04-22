
using System.Diagnostics.CodeAnalysis;

namespace ConfigurationRepository.SqlClient;

public class SqlClientJsonConfigurationSource : ConfigurationRepositorySource
{
    [DisallowNull]
    public string? ConnectionString { get; set; }

    [DisallowNull]
    public string? Key { get; set; }
}
