using System.Text.Json;

namespace PalworldServerManager.Core.Configuration;

/// <summary>
/// Loads/saves app-level config (e.g. the server install path) as JSON in the
/// current user's per-user application data folder, separate from the game
/// server's own settings files.
/// </summary>
public class AppConfigService : IAppConfigService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _configFilePath;
    private readonly object _lock = new();
    private AppConfig? _current;

    public AppConfigService()
        : this(GetDefaultConfigFilePath())
    {
    }

    public AppConfigService(string configFilePath)
    {
        _configFilePath = configFilePath;
    }

    public static string GetDefaultConfigFilePath()
    {
        var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appDataDir, "PalworldServerManager", "config.json");
    }

    public AppConfig Current
    {
        get
        {
            lock (_lock)
            {
                _current ??= LoadFromDisk();
                return _current;
            }
        }
    }

    public async Task SaveAsync(AppConfig config, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(_configFilePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(config, JsonOptions);
        await File.WriteAllTextAsync(_configFilePath, json, cancellationToken);

        lock (_lock)
        {
            _current = config;
        }
    }

    private AppConfig LoadFromDisk()
    {
        if (!File.Exists(_configFilePath))
        {
            return new AppConfig();
        }

        try
        {
            var json = File.ReadAllText(_configFilePath);
            return JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? new AppConfig();
        }
        catch (JsonException)
        {
            // Corrupt config file — fall back to defaults rather than crashing the app.
            return new AppConfig();
        }
    }
}
