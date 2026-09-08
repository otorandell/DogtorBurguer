using System.Collections.Generic;

namespace DogtorBurguer
{
    /// <summary>PT strings. One entry per LocKey — Loc's editor boot check screams on gaps.
    /// The font renders ALL CAPS regardless of case (Panton Black Caps).</summary>
    public static class Strings_PT
    {
        public static readonly Dictionary<LocKey, string> Table = new()
        {
            { LocKey.SettingsTitle, "AJUSTES" },
            { LocKey.SettingsSoundOn, "Som: SIM" },
            { LocKey.SettingsSoundOff, "Som: NÃO" },
            { LocKey.SettingsControlsDrag, "Controles: Arrastar" },
            { LocKey.SettingsControlsTap, "Controles: Tocar" },
            { LocKey.SettingsStartLevel, "INÍCIO: NVL {0}" },
            { LocKey.SettingsQuit, "Voltar ao Menu" },
            { LocKey.SettingsLanguage, "Idioma: {0}" },
        };
    }
}
