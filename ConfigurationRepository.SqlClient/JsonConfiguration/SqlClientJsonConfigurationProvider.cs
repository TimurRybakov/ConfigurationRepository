
namespace ConfigurationRepository.SqlClient;

public class SqlClientJsonConfigurationProvider(SqlClientJsonConfigurationSource source) :
    ConfigurationRepositoryProvider(source)
{
    /// <summary>
    /// The source settings for this provider.
    /// </summary>
    public new SqlClientJsonConfigurationSource Source =>
        (SqlClientJsonConfigurationSource)base.Source;
}
