using System.Collections.Generic;

namespace DogtorBurguer
{
    /// <summary>FR strings. One entry per LocKey — Loc's editor boot check screams on gaps.
    /// The font renders ALL CAPS regardless of case (Panton Black Caps).</summary>
    public static class Strings_FR
    {
        public static readonly Dictionary<LocKey, string> Table = new()
        {
            { LocKey.SettingsTitle, "RÉGLAGES" },
            { LocKey.SettingsSoundOn, "Son: OUI" },
            { LocKey.SettingsSoundOff, "Son: NON" },
            { LocKey.SettingsControlsDrag, "Contrôles: Glisser" },
            { LocKey.SettingsControlsTap, "Contrôles: Toucher" },
            { LocKey.SettingsStartLevel, "DÉBUT: NIV {0}" },
            { LocKey.SettingsQuit, "Retour au Menu" },
            { LocKey.SettingsLanguage, "Langue: {0}" },
        };
    }
}
