
namespace ConfigurationRepository.SqlClient;

public class SqlClientConfigurationProvider : ConfigurationRepositoryProvider
{
    public SqlClientConfigurationProvider(SqlClientConfigurationSource source) : base(source)
    {
    }

    /// <summary>
    /// The source settings for this provider.
    /// </summary>
    public new SqlClientConfigurationSource Source => (SqlClientConfigurationSource)base.Source;
}
