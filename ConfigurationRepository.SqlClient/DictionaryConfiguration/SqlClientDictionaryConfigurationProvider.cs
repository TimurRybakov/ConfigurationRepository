
namespace ConfigurationRepository.SqlClient;

public class SqlClientDictionaryConfigurationProvider(SqlClientDictionaryConfigurationSource source) :
    ConfigurationRepositoryProvider(source)
{
    /// <summary>
    /// The source settings for this provider.
    /// </summary>
    public new SqlClientDictionaryConfigurationSource Source =>
        (SqlClientDictionaryConfigurationSource)base.Source;
}
