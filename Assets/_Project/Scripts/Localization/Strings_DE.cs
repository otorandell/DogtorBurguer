using System.Collections.Generic;

namespace DogtorBurguer
{
    /// <summary>DE strings. One entry per LocKey — Loc's editor boot check screams on gaps.
    /// The font renders ALL CAPS regardless of case (Panton Black Caps).</summary>
    public static class Strings_DE
    {
        public static readonly Dictionary<LocKey, string> Table = new()
        {
            { LocKey.SettingsTitle, "EINSTELLUNGEN" },
            { LocKey.SettingsSoundOn, "Ton: AN" },
            { LocKey.SettingsSoundOff, "Ton: AUS" },
            { LocKey.SettingsControlsDrag, "Steuerung: Ziehen" },
            { LocKey.SettingsControlsTap, "Steuerung: Tippen" },
            { LocKey.SettingsStartLevel, "START: LVL {0}" },
            { LocKey.SettingsQuit, "Zum Menü" },
            { LocKey.SettingsLanguage, "Sprache: {0}" },
        };
    }
}
