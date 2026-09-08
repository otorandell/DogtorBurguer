using System.Collections.Generic;

namespace DogtorBurguer
{
    /// <summary>ES strings. One entry per LocKey — Loc's editor boot check screams on gaps.
    /// The font renders ALL CAPS regardless of case (Panton Black Caps).</summary>
    public static class Strings_ES
    {
        public static readonly Dictionary<LocKey, string> Table = new()
        {
            { LocKey.SettingsTitle, "AJUSTES" },
            { LocKey.SettingsSoundOn, "Sonido: SÍ" },
            { LocKey.SettingsSoundOff, "Sonido: NO" },
            { LocKey.SettingsControlsDrag, "Controles: Arrastrar" },
            { LocKey.SettingsControlsTap, "Controles: Tocar" },
            { LocKey.SettingsStartLevel, "INICIO: NVL {0}" },
            { LocKey.SettingsQuit, "Salir al Menú" },
            { LocKey.SettingsLanguage, "Idioma: {0}" },
        };
    }
}
