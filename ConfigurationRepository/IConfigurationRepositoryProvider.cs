namespace ConfigurationRepository;

public interface IConfigurationRepositoryProvider : IDisposable
{
    public void Reload();
}
