namespace DogtorBurguer
{
    /// <summary>Typed keys for every player-facing string (no magic strings — exhaustive,
    /// refactor-safe). Every Strings_XX table must carry every key; Loc's editor boot check
    /// screams otherwise. Keys with {0} placeholders are formatted via Loc.Format.</summary>
    public enum LocKey
    {
        SettingsTitle,
        SettingsSoundOn,
        SettingsSoundOff,
        SettingsControlsDrag,
        SettingsControlsTap,
        SettingsStartLevel,  // "START: LVL {0}"
        SettingsQuit,
        SettingsLanguage,    // "Language: {0}" — {0} = the native name
    }
}
