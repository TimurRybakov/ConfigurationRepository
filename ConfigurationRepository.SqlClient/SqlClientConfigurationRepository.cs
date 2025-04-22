namespace ConfigurationRepository.SqlClient;

public abstract class SqlClientConfigurationRepository
{
    /// <summary>
    /// The connection string to connect to the database with configuration.
    /// </summary>
    public string? ConnectionString { get; set; }
}
