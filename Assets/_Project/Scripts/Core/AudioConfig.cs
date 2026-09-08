namespace DogtorBurguer
{
    /// <summary>
    /// Audio mix defaults — owned by the audio designer. The sixth config file in the
    /// architecture (Constants / GameplayConfig / MonetizationConfig / AnimConfig /
    /// UIStyles / AudioConfig). Procedural SFX math stays at the implementation;
    /// only tunable mix values live here.
    /// </summary>
    public static class AudioConfig
    {
        public const float DEFAULT_MUSIC_VOLUME = 0.5f;

        // ---- Chip-style SFX voicing (2026-09-08) ----
        // The smooth sine tones read as generic; these run every generated sound through an
        // 8-bit chain in AudioManager.GenerateSound — same melodies/envelopes, chip timbre
        // (and a better match for the chiptune music tracks). One toggle to A/B or revert.
        public const bool CHIP_STYLE = true;        // false = the original smooth tones
        public const float CHIP_DRIVE = 2.2f;       // pre-clamp gain: pushes the dominant voice toward a square wave
        public const int CHIP_SAMPLE_RATE = 11025;  // sample-hold target Hz (lower = grittier aliasing)
        public const int CHIP_LEVELS = 16;          // amplitude steps per side (~4 bits of crunch; higher = subtler)
    }
}
