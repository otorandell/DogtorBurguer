using System.Collections.Generic;

namespace DogtorBurguer
{
    /// <summary>IT strings. One entry per LocKey — Loc's editor boot check screams on gaps.
    /// The font renders ALL CAPS regardless of case (Panton Black Caps).</summary>
    public static class Strings_IT
    {
        public static readonly Dictionary<LocKey, string> Table = new()
        {
            { LocKey.SettingsTitle, "IMPOSTAZIONI" },
            { LocKey.SettingsSoundOn, "Audio: SÌ" },
            { LocKey.SettingsSoundOff, "Audio: NO" },
            { LocKey.SettingsControlsDrag, "Controlli: Trascina" },
            { LocKey.SettingsControlsTap, "Controlli: Tocca" },
            { LocKey.SettingsStartLevel, "INIZIO: LIV {0}" },
            { LocKey.SettingsQuit, "Torna al Menu" },
            { LocKey.SettingsLanguage, "Lingua: {0}" },
        };
    }
}
