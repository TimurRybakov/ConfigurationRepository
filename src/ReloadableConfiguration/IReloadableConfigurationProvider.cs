namespace ConfigurationRepository;

/// <summary>
/// Marker interface that marks <see cref="ConfigurationReloader"/> as reloadable.
/// Reloadable providers are managed by <see cref="ConfigurationReloader"/> hosted srvice.
/// </summary>
public interface IReloadableConfigurationProvider : IDisposable
{
    /// <summary>
    /// Triggers provider reload.
    /// </summary>
    void Reload();

    /// <summary>
    /// Turns on/off periodical reload by <see cref="ConfigurationReloader"/> hosted service.
    /// </summary>
    bool PeriodicalReload { get; set; }
}
