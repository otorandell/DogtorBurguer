using System.Collections.Generic;

namespace DogtorBurguer
{
    /// <summary>EN strings. One entry per LocKey — Loc's editor boot check screams on gaps.
    /// The font renders ALL CAPS regardless of case (Panton Black Caps).</summary>
    public static class Strings_EN
    {
        public static readonly Dictionary<LocKey, string> Table = new()
        {
            { LocKey.SettingsTitle, "SETTINGS" },
            { LocKey.SettingsSoundOn, "Sound: ON" },
            { LocKey.SettingsSoundOff, "Sound: OFF" },
            { LocKey.SettingsControlsDrag, "Controls: Drag" },
            { LocKey.SettingsControlsTap, "Controls: Tap" },
            { LocKey.SettingsStartLevel, "START: LVL {0}" },
            { LocKey.SettingsQuit, "Quit to Menu" },
            { LocKey.SettingsLanguage, "Language: {0}" },
        };
    }
}
