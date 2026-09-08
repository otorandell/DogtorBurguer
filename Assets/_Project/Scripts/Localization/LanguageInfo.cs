using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>Per-language metadata: the native display name (Settings row), the Settings
    /// cycle, and the device-language mapping for the first-run default.</summary>
    public static class LanguageInfo
    {
        public const int Count = 7;

        public static string NativeName(Language language) => language switch
        {
            Language.Spanish => "Español",
            Language.Portuguese => "Português",
            Language.German => "Deutsch",
            Language.French => "Français",
            Language.Italian => "Italiano",
            Language.Turkish => "Türkçe",
            _ => "English",
        };

        public static Language Next(Language language) => (Language)(((int)language + 1) % Count);

        /// <summary>Maps the device language to a shipped one — anything unsupported is English.</summary>
        public static Language FromSystem(SystemLanguage system) => system switch
        {
            SystemLanguage.Spanish => Language.Spanish,
            SystemLanguage.Portuguese => Language.Portuguese,
            SystemLanguage.German => Language.German,
            SystemLanguage.French => Language.French,
            SystemLanguage.Italian => Language.Italian,
            SystemLanguage.Turkish => Language.Turkish,
            _ => Language.English,
        };
    }
}
