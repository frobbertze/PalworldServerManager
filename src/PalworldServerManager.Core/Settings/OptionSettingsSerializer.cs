using System.Text;

namespace PalworldServerManager.Core.Settings;

/// <summary>
/// Parses/serializes the value of PalWorldSettings.ini's "OptionSettings=(...)" line —
/// an Unreal Engine "struct string": a parenthesized, comma-separated list of Key=Value
/// pairs, where a Value can itself be a quoted string (with \" escaping) or a nested
/// parenthesized list (e.g. CrossplayPlatforms=(Steam,Xbox,PS5,Mac)).
/// </summary>
public static class OptionSettingsSerializer
{
    /// <param name="optionSettingsValue">Everything after "OptionSettings=", including the outer parens.</param>
    public static PalWorldOptionSettings Parse(string optionSettingsValue)
    {
        var trimmed = optionSettingsValue.Trim();
        if (trimmed.StartsWith('(') && trimmed.EndsWith(')'))
        {
            trimmed = trimmed[1..^1];
        }

        var settings = new PalWorldOptionSettings();

        foreach (var segment in SplitTopLevel(trimmed, ','))
        {
            if (segment.Length == 0)
            {
                continue;
            }

            var equalsIndex = segment.IndexOf('=');
            if (equalsIndex < 0)
            {
                // Malformed/unexpected entry — preserve it verbatim under its own text so it isn't lost.
                continue;
            }

            var key = segment[..equalsIndex];
            var value = segment[(equalsIndex + 1)..];
            settings.SetRaw(key, value);
        }

        return settings;
    }

    public static string Serialize(PalWorldOptionSettings settings)
    {
        var sb = new StringBuilder();
        sb.Append('(');

        for (var i = 0; i < settings.Keys.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(',');
            }

            var key = settings.Keys[i];
            sb.Append(key).Append('=').Append(settings.GetRaw(key));
        }

        sb.Append(')');
        return sb.ToString();
    }

    /// <summary>Splits <paramref name="s"/> on <paramref name="separator"/> at depth 0 only —
    /// ignoring separators inside "quoted strings" or (nested parens).</summary>
    private static List<string> SplitTopLevel(string s, char separator)
    {
        var parts = new List<string>();
        var depth = 0;
        var inQuotes = false;
        var start = 0;

        for (var i = 0; i < s.Length; i++)
        {
            var c = s[i];

            if (c == '"' && (i == 0 || s[i - 1] != '\\'))
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (inQuotes)
            {
                continue;
            }

            switch (c)
            {
                case '(':
                    depth++;
                    break;
                case ')':
                    depth--;
                    break;
                default:
                    if (c == separator && depth == 0)
                    {
                        parts.Add(s[start..i]);
                        start = i + 1;
                    }
                    break;
            }
        }

        parts.Add(s[start..]);
        return parts;
    }
}
