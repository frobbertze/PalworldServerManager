namespace PalworldServerManager.Core.Settings;

public enum SettingControlType
{
    Toggle,
    FloatSlider,
    IntNumber,
    Text,
    Dropdown,
}

/// <summary>Friendly metadata for one OptionSettings key, used to render a nicer "Quick Settings" control for it.</summary>
public record SettingDefinition(
    string Key,
    string Label,
    string Category,
    SettingControlType ControlType,
    float Min = 0,
    float Max = 5,
    float Step = 0.1f,
    string[]? DropdownOptions = null,
    string? Description = null);
