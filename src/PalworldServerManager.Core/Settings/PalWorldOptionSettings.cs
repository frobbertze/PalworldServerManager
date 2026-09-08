using System.Globalization;

namespace PalworldServerManager.Core.Settings;

/// <summary>
/// The parsed contents of the "OptionSettings=(...)" struct from PalWorldSettings.ini,
/// as an ordered list of key -> raw (still game-formatted) value strings.
///
/// Values are kept in their raw, game-formatted form (e.g. "1.000000", "True", "\"Some Name\"",
/// "(Steam,Xbox,PS5,Mac)") so that any key we don't have a typed accessor for round-trips
/// byte-for-byte instead of being dropped or reformatted.
/// </summary>
public class PalWorldOptionSettings
{
    private readonly List<string> _order = [];
    private readonly Dictionary<string, string> _values = new(StringComparer.Ordinal);

    public IReadOnlyList<string> Keys => _order;

    public bool ContainsKey(string key) => _values.ContainsKey(key);

    public string? GetRaw(string key) => _values.GetValueOrDefault(key);

    public void SetRaw(string key, string rawValue)
    {
        if (!_values.ContainsKey(key))
        {
            _order.Add(key);
        }

        _values[key] = rawValue;
    }

    public bool GetBool(string key) => GetRaw(key) == "True";

    public void SetBool(string key, bool value) => SetRaw(key, value ? "True" : "False");

    public float GetFloat(string key)
    {
        var raw = GetRaw(key);
        return raw is null ? 0f : float.Parse(raw, CultureInfo.InvariantCulture);
    }

    public void SetFloat(string key, float value) => SetRaw(key, value.ToString("F6", CultureInfo.InvariantCulture));

    public int GetInt(string key)
    {
        var raw = GetRaw(key);
        return raw is null ? 0 : int.Parse(raw, CultureInfo.InvariantCulture);
    }

    public void SetInt(string key, int value) => SetRaw(key, value.ToString(CultureInfo.InvariantCulture));

    /// <summary>Unquotes/unescapes a quoted-string value. Returns the raw value unchanged if it isn't quoted.</summary>
    public string GetString(string key)
    {
        var raw = GetRaw(key);
        if (raw is null || raw.Length < 2 || raw[0] != '"' || raw[^1] != '"')
        {
            return raw ?? string.Empty;
        }

        return raw[1..^1].Replace("\\\"", "\"");
    }

    public void SetString(string key, string value)
    {
        var escaped = value.Replace("\"", "\\\"");
        SetRaw(key, $"\"{escaped}\"");
    }
}
