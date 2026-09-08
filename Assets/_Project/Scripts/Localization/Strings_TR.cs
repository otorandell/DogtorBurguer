using System.Collections.Generic;

namespace DogtorBurguer
{
    /// <summary>TR strings. One entry per LocKey — Loc's editor boot check screams on gaps.
    /// The font renders ALL CAPS regardless of case (Panton Black Caps).</summary>
    public static class Strings_TR
    {
        public static readonly Dictionary<LocKey, string> Table = new()
        {
            { LocKey.SettingsTitle, "AYARLAR" },
            { LocKey.SettingsSoundOn, "Ses: AÇIK" },
            { LocKey.SettingsSoundOff, "Ses: KAPALI" },
            { LocKey.SettingsControlsDrag, "Kontrol: Sürükle" },
            { LocKey.SettingsControlsTap, "Kontrol: Dokun" },
            { LocKey.SettingsStartLevel, "BAŞLANGIÇ: SVY {0}" },
            { LocKey.SettingsQuit, "Menüye Dön" },
            { LocKey.SettingsLanguage, "Dil: {0}" },
        };
    }
}
