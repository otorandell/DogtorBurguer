using System.Collections.Generic;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>Serves the current language's string per LocKey (the Theme of text): one table
    /// file per language, English fallback + loud error on a missing key. The current language
    /// lives in SaveDataManager (auto-detected from the device on first run).</summary>
    public static class Loc
    {
        private static readonly Dictionary<Language, IReadOnlyDictionary<LocKey, string>> Tables = new()
        {
            { Language.English, Strings_EN.Table },
            { Language.Spanish, Strings_ES.Table },
            { Language.Portuguese, Strings_PT.Table },
            { Language.German, Strings_DE.Table },
            { Language.French, Strings_FR.Table },
            { Language.Italian, Strings_IT.Table },
            { Language.Turkish, Strings_TR.Table },
        };

        public static Language Current => SaveDataManager.Instance != null
            ? SaveDataManager.Instance.Language
            : Language.English;

        public static string Get(LocKey key)
        {
            if (Tables[Current].TryGetValue(key, out string value))
                return value;

            Debug.LogError($"[Loc] {Current} table is missing {key} — falling back to English");
            return Strings_EN.Table.TryGetValue(key, out string english) ? english : key.ToString();
        }

        public static string Format(LocKey key, params object[] args) => string.Format(Get(key), args);

#if UNITY_EDITOR
        // Editor-only drift guard: every table must carry every key (no silent defaults).
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void ValidateTables()
        {
            foreach (var pair in Tables)
                foreach (LocKey key in System.Enum.GetValues(typeof(LocKey)))
                    if (!pair.Value.ContainsKey(key))
                        Debug.LogError($"[Loc] {pair.Key} table is missing key {key}");
        }
#endif
    }
}
