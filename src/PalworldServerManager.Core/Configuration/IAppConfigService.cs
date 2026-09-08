namespace PalworldServerManager.Core.Configuration;

public interface IAppConfigService
{
    /// <summary>Returns the currently loaded config (loaded from disk on first access).</summary>
    AppConfig Current { get; }

    Task SaveAsync(AppConfig config, CancellationToken cancellationToken = default);
}
