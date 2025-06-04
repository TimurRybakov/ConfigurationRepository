namespace ConfigurationRepository;

public interface IReloadableConfigurationProvider : IDisposable
{
    public void Reload();

    public bool PeriodicalReload { get; set; }
}
