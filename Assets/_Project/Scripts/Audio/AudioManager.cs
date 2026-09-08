using System;
using UnityEngine;

namespace DogtorBurguer
{
    public class AudioManager : Singleton<AudioManager>
    {
        private AudioSource _sfxSource;
        private AudioSource _squeezSource;

        private AudioClip _matchClip;
        private AudioClip _burgerPoorClip;
        private AudioClip _burgerSmallClip;
        private AudioClip _burgerMediumClip;
        private AudioClip _burgerLargeClip;
        private AudioClip _burgerMegaClip;
        private AudioClip _burgerMaxClip;
        private AudioClip _levelUpClip;
        private AudioClip _gameOverClip;
        private AudioClip _squeezeClip;
        private AudioClip _fastDropClip;
        private AudioClip _earlySpawnClip;
        private AudioClip _challengeMatchClip;
        private AudioClip _challengeLevelUpClip;
        private AudioClip _uiTapClip;
        private AudioClip _purchaseClip;
        private AudioClip _equipClip;
        private AudioClip _denyClip;
        private AudioClip _fairyAppearClip;
        private AudioClip _consumableCollectClip;
        private AudioClip _consumableKetchupClip;
        private AudioClip _consumableMustardClip;
        private AudioClip _consumableSkewerClip;
        private AudioClip _consumableFizzleClip;

        [Header("Authored Clip Overrides (optional — used instead of the generated clip if assigned)")]
        [SerializeField] private AudioClip _matchOverride;
        [SerializeField] private AudioClip _burgerPoorOverride;
        [SerializeField] private AudioClip _burgerSmallOverride;
        [SerializeField] private AudioClip _burgerMediumOverride;
        [SerializeField] private AudioClip _burgerLargeOverride;
        [SerializeField] private AudioClip _burgerMegaOverride;
        [SerializeField] private AudioClip _burgerMaxOverride;
        [SerializeField] private AudioClip _levelUpOverride;
        [SerializeField] private AudioClip _gameOverOverride;
        [SerializeField] private AudioClip _squeezeOverride;
        [SerializeField] private AudioClip _fastDropOverride;
        [SerializeField] private AudioClip _earlySpawnOverride;
        [SerializeField] private AudioClip _challengeMatchOverride;
        [SerializeField] private AudioClip _challengeLevelUpOverride;
        [SerializeField] private AudioClip _uiTapOverride;
        [SerializeField] private AudioClip _purchaseOverride;
        [SerializeField] private AudioClip _equipOverride;
        [SerializeField] private AudioClip _denyOverride;
        [SerializeField] private AudioClip _fairyAppearOverride;
        [SerializeField] private AudioClip _consumableCollectOverride;
        [SerializeField] private AudioClip _consumableKetchupOverride;
        [SerializeField] private AudioClip _consumableMustardOverride;
        [SerializeField] private AudioClip _consumableSkewerOverride;
        [SerializeField] private AudioClip _consumableFizzleOverride;

        private const int SAMPLE_RATE = 44100;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) return;

            // Ensure an AudioListener exists in the scene
            if (FindAnyObjectByType<AudioListener>() == null)
            {
                Camera cam = Camera.main;
                if (cam != null)
                    cam.gameObject.AddComponent<AudioListener>();
                else
                    gameObject.AddComponent<AudioListener>();
            }

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;

            _squeezSource = gameObject.AddComponent<AudioSource>();
            _squeezSource.playOnAwake = false;

            GenerateClips();
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            GridManager gridManager = GridManager.Instance;
            if (gridManager != null)
            {
                gridManager.OnMatchEffect += HandleMatch;
                gridManager.OnBurgerEffect += HandleBurger;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnLevelChanged += HandleLevelUp;
                GameManager.Instance.OnStateChanged += HandleStateChanged;
            }
        }

        private void HandleMatch(Vector3 pos, int points)
        {
            PlayClip(_matchClip, 0.5f);
        }

        private void HandleBurger(Vector3 pos, int points, string name, int ingredientCount)
        {
            AudioClip clip = Scoring.GetBurgerTier(ingredientCount) switch
            {
                BurgerTier.Max => _burgerMaxClip,
                BurgerTier.Mega => _burgerMegaClip,
                BurgerTier.Large => _burgerLargeClip,
                BurgerTier.Medium => _burgerMediumClip,
                BurgerTier.Small => _burgerSmallClip,
                _ => _burgerPoorClip,
            };

            PlayClip(clip, 0.7f);
        }

        private void HandleLevelUp(int level)
        {
            if (level > 1)
                PlayClip(_levelUpClip, 0.6f);
        }

        private void HandleStateChanged(GameState state)
        {
            if (state == GameState.GameOver)
                PlayClip(_gameOverClip, 0.8f);
        }

        private void PlayClip(AudioClip clip, float volume)
        {
            if (clip != null && _sfxSource != null)
                _sfxSource.PlayOneShot(clip, volume);
        }

        private void GenerateClips()
        {
            _matchClip = Resolve(_matchOverride, "Match", 0.15f, GenerateMatchSamples);
            _burgerPoorClip = Resolve(_burgerPoorOverride, "BurgerPoor", 0.3f, GenerateBurgerPoorSamples);
            _burgerSmallClip = Resolve(_burgerSmallOverride, "BurgerSmall", 0.25f, GenerateBurgerSmallSamples);
            _burgerMediumClip = Resolve(_burgerMediumOverride, "BurgerMedium", 0.4f, GenerateBurgerMediumSamples);
            _burgerLargeClip = Resolve(_burgerLargeOverride, "BurgerLarge", 0.5f, GenerateBurgerLargeSamples);
            _burgerMegaClip = Resolve(_burgerMegaOverride, "BurgerMega", 0.6f, GenerateBurgerMegaSamples);
            _burgerMaxClip = Resolve(_burgerMaxOverride, "BurgerMax", 0.8f, GenerateBurgerMaxSamples);
            _levelUpClip = Resolve(_levelUpOverride, "LevelUp", 0.5f, GenerateLevelUpSamples);
            _gameOverClip = Resolve(_gameOverOverride, "GameOver", 0.8f, GenerateGameOverSamples);
            _squeezeClip = Resolve(_squeezeOverride, "Squeeze", 0.1f, GenerateSqueezeSamples);
            _fastDropClip = Resolve(_fastDropOverride, "FastDrop", 0.12f, GenerateFastDropSamples);
            _earlySpawnClip = Resolve(_earlySpawnOverride, "EarlySpawn", 0.15f, GenerateEarlySpawnSamples);
            _challengeMatchClip = Resolve(_challengeMatchOverride, "ChallengeMatch", 0.35f, GenerateChallengeMatchSamples);
            _challengeLevelUpClip = Resolve(_challengeLevelUpOverride, "ChallengeLevelUp", 0.55f, GenerateChallengeLevelUpSamples);
            _uiTapClip = Resolve(_uiTapOverride, "UiTap", 0.06f, GenerateUiTapSamples);
            _purchaseClip = Resolve(_purchaseOverride, "Purchase", 0.3f, GeneratePurchaseSamples);
            _equipClip = Resolve(_equipOverride, "Equip", 0.16f, GenerateEquipSamples);
            _denyClip = Resolve(_denyOverride, "Deny", 0.16f, GenerateDenySamples);
            _fairyAppearClip = Resolve(_fairyAppearOverride, "FairyAppear", 0.45f, GenerateFairyAppearSamples);
            _consumableCollectClip = Resolve(_consumableCollectOverride, "ConsumableCollect", 0.18f, GenerateConsumableCollectSamples);
            _consumableKetchupClip = Resolve(_consumableKetchupOverride, "ConsumableKetchup", 0.18f, GenerateKetchupSamples);
            _consumableMustardClip = Resolve(_consumableMustardOverride, "ConsumableMustard", 0.16f, GenerateMustardSamples);
            _consumableSkewerClip = Resolve(_consumableSkewerOverride, "ConsumableSkewer", 0.20f, GenerateSkewerSamples);
            _consumableFizzleClip = Resolve(_consumableFizzleOverride, "ConsumableFizzle", 0.16f, GenerateFizzleSamples);
        }

        /// <summary>Use the authored clip if one is assigned in the inspector, else generate procedurally (F-2).</summary>
        private AudioClip Resolve(AudioClip authored, string name, float duration, Func<float, int, float> sampleFunc)
        {
            return authored != null ? authored : GenerateSound(name, duration, sampleFunc);
        }

        private AudioClip GenerateSound(string name, float duration, Func<float, int, float> sampleFunc)
        {
            int sampleCount = (int)(SAMPLE_RATE * duration);
            float[] samples = new float[sampleCount];

            // Chip-style voicing (2026-09-08, AudioConfig.CHIP_STYLE): the same melodies pushed
            // through an 8-bit chain — (1) sample-hold at a low virtual rate for the aliasing
            // grit, (2) drive + hard clamp so the dominant voice squares off like a pulse wave,
            // (3) amplitude quantization for the crunch. The 0.8 ceiling keeps headroom under
            // the per-call PlayClip volumes (an algorithmic constant, not a tuning value).
            int hold = Mathf.Max(1, SAMPLE_RATE / AudioConfig.CHIP_SAMPLE_RATE);
            for (int i = 0; i < sampleCount; i++)
            {
                if (!AudioConfig.CHIP_STYLE)
                {
                    samples[i] = sampleFunc(duration, i);
                    continue;
                }
                float s = sampleFunc(duration, i - (i % hold));
                s = Mathf.Clamp(s * AudioConfig.CHIP_DRIVE, -0.8f, 0.8f);
                samples[i] = Mathf.Round(s * AudioConfig.CHIP_LEVELS) / AudioConfig.CHIP_LEVELS;
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, SAMPLE_RATE, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Short ascending beep for ingredient match
        /// </summary>
        private float GenerateMatchSamples(float duration, int i)
        {
            float t = (float)i / SAMPLE_RATE;
            float freq = Mathf.Lerp(600f, 900f, t / duration);
            float envelope = 1f - (t / duration);
            return Mathf.Sin(2f * Mathf.PI * freq * t) * envelope;
        }

        /// <summary>
        /// Poor burger (just bread): sad descending "wah wah" (E4, C4)
        /// </summary>
        private float GenerateBurgerPoorSamples(float duration, int i)
        {
            float t = (float)i / SAMPLE_RATE;
            float[] notes = { 330f, 262f };
            float noteLength = duration / notes.Length;
            int noteIndex = Mathf.Min((int)(t / noteLength), notes.Length - 1);
            float noteT = (t - noteIndex * noteLength) / noteLength;
            float envelope = (1f - noteT * 0.7f) * (1f - t / duration * 0.5f);
            float freq = notes[noteIndex];
            return Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.5f;
        }

        /// <summary>
        /// Small burger: 2 quick ascending notes (C5, G5)
        /// </summary>
        private float GenerateBurgerSmallSamples(float duration, int i)
        {
            float t = (float)i / SAMPLE_RATE;
            float[] notes = { 523f, 784f };
            float noteLength = duration / notes.Length;
            int noteIndex = Mathf.Min((int)(t / noteLength), notes.Length - 1);
            float noteT = (t - noteIndex * noteLength) / noteLength;
            float envelope = 1f - noteT * 0.6f;
            float freq = notes[noteIndex];
            return Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.7f;
        }

        /// <summary>
        /// Medium burger: ascending arpeggio (C5, E5, G5, C6)
        /// </summary>
        private float GenerateBurgerMediumSamples(float duration, int i)
        {
            float t = (float)i / SAMPLE_RATE;
            float[] notes = { 523f, 659f, 784f, 1047f };
            float noteLength = duration / notes.Length;
            int noteIndex = Mathf.Min((int)(t / noteLength), notes.Length - 1);
            float noteT = (t - noteIndex * noteLength) / noteLength;
            float envelope = 1f - noteT * 0.5f;
            float freq = notes[noteIndex];
            return Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.8f;
        }

        /// <summary>
        /// Large burger: richer arpeggio with harmonics (C5, E5, G5, B5, C6)
        /// </summary>
        private float GenerateBurgerLargeSamples(float duration, int i)
        {
            float t = (float)i / SAMPLE_RATE;
            float[] notes = { 523f, 659f, 784f, 988f, 1047f };
            float noteLength = duration / notes.Length;
            int noteIndex = Mathf.Min((int)(t / noteLength), notes.Length - 1);
            float noteT = (t - noteIndex * noteLength) / noteLength;
            float envelope = (1f - noteT * 0.4f) * (1f - t / duration * 0.3f);
            float freq = notes[noteIndex];
            return (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.7f
                  + Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * 0.3f) * envelope * 0.8f;
        }

        /// <summary>
        /// Mega burger: two-octave arpeggio with harmonics (C5, E5, G5, C6, E6, G6)
        /// </summary>
        private float GenerateBurgerMegaSamples(float duration, int i)
        {
            float t = (float)i / SAMPLE_RATE;
            float[] notes = { 523f, 659f, 784f, 1047f, 1319f, 1568f };
            float noteLength = duration / notes.Length;
            int noteIndex = Mathf.Min((int)(t / noteLength), notes.Length - 1);
            float noteT = (t - noteIndex * noteLength) / noteLength;
            float envelope = (1f - noteT * 0.3f) * (1f - t / duration * 0.2f);
            float freq = notes[noteIndex];
            return (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.6f
                  + Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * 0.25f
                  + Mathf.Sin(2f * Mathf.PI * freq * 3f * t) * 0.15f) * envelope * 0.8f;
        }

        /// <summary>
        /// Max burger (DOKTOR BURGUER): triumphant fanfare with full harmonics
        /// (C5, G5, C6, E6, G6, C7) layered with octave and fifth
        /// </summary>
        private float GenerateBurgerMaxSamples(float duration, int i)
        {
            float t = (float)i / SAMPLE_RATE;
            float[] notes = { 523f, 784f, 1047f, 1319f, 1568f, 2093f };
            float noteLength = duration / notes.Length;
            int noteIndex = Mathf.Min((int)(t / noteLength), notes.Length - 1);
            float noteT = (t - noteIndex * noteLength) / noteLength;
            float envelope = (1f - noteT * 0.2f) * (1f - t / duration * 0.15f);
            float freq = notes[noteIndex];
            // Rich layered sound: fundamental + octave + fifth + two octaves
            float signal = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.4f
                         + Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * 0.25f
                         + Mathf.Sin(2f * Mathf.PI * freq * 1.5f * t) * 0.2f
                         + Mathf.Sin(2f * Mathf.PI * freq * 4f * t) * 0.15f;
            return signal * envelope * 0.85f;
        }

        /// <summary>
        /// Cheerful ascending tones for level up (A4, C#5, E5, A5, C#6)
        /// </summary>
        private float GenerateLevelUpSamples(float duration, int i)
        {
            float t = (float)i / SAMPLE_RATE;
            float[] notes = { 440f, 554f, 659f, 880f, 1109f };
            float noteLength = duration / notes.Length;
            int noteIndex = Mathf.Min((int)(t / noteLength), notes.Length - 1);
            float noteT = (t - noteIndex * noteLength) / noteLength;
            float envelope = (1f - noteT * 0.3f) * (1f - t / duration * 0.5f);
            float freq = notes[noteIndex];
            return (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.7f
                  + Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * 0.3f) * envelope * 0.7f;
        }

        /// <summary>
        /// Short descending beep for burger squeeze
        /// </summary>
        private float GenerateSqueezeSamples(float duration, int i)
        {
            float t = (float)i / SAMPLE_RATE;
            float freq = Mathf.Lerp(800f, 500f, t / duration);
            float envelope = 1f - (t / duration);
            return Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.7f;
        }

        public void PlaySqueeze(float pitch = 1f)
        {
            if (_squeezeClip != null && _squeezSource != null)
            {
                _squeezSource.pitch = pitch;
                _squeezSource.clip = _squeezeClip;
                _squeezSource.volume = 0.5f;
                _squeezSource.Play();
            }
        }

        /// <summary>
        /// Quick descending whoosh for fast drop
        /// </summary>
        private float GenerateFastDropSamples(float duration, int i)
        {
            float t = (float)i / SAMPLE_RATE;
            float freq = Mathf.Lerp(1200f, 300f, t / duration);
            float envelope = 1f - (t / duration);
            return Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.6f;
        }

        public void PlayFastDrop()
        {
            PlayClip(_fastDropClip, 0.5f);
        }

        /// <summary>
        /// Quick pop for early spawn
        /// </summary>
        private float GenerateEarlySpawnSamples(float duration, int i)
        {
            float t = (float)i / SAMPLE_RATE;
            float freq = Mathf.Lerp(500f, 1000f, t / duration);
            float envelope = (1f - t / duration) * (1f - t / duration);
            return Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.6f;
        }

        public void PlayEarlySpawn()
        {
            PlayClip(_earlySpawnClip, 0.6f);
        }

        /// <summary>
        /// Quick celebratory chord for challenge match (C5+E5+G5 simultaneous, then octave C6)
        /// </summary>
        private float GenerateChallengeMatchSamples(float duration, int i)
        {
            float t = (float)i / SAMPLE_RATE;
            float split = duration * 0.6f;
            float envelope;
            float signal;

            if (t < split)
            {
                // Major chord hit
                envelope = (1f - t / split * 0.4f);
                signal = Mathf.Sin(2f * Mathf.PI * 523f * t) * 0.35f  // C5
                       + Mathf.Sin(2f * Mathf.PI * 659f * t) * 0.3f   // E5
                       + Mathf.Sin(2f * Mathf.PI * 784f * t) * 0.25f  // G5
                       + Mathf.Sin(2f * Mathf.PI * 1047f * t) * 0.1f; // C6 shimmer
            }
            else
            {
                // Resolve to octave
                float t2 = (t - split) / (duration - split);
                envelope = 1f - t2;
                signal = Mathf.Sin(2f * Mathf.PI * 1047f * t) * 0.5f   // C6
                       + Mathf.Sin(2f * Mathf.PI * 2093f * t) * 0.2f;  // C7
            }
            return signal * envelope * 0.7f;
        }

        public void PlayChallengeMatch()
        {
            PlayClip(_challengeMatchClip, 0.7f);
        }

        /// <summary>
        /// Rising C-major arpeggio with a shimmer tail (C5 E5 G5 C6 E6) — the MULT level-up.
        /// Deliberately distinct from the game level-up's A-major run and the match chord
        /// (they read as one reused sound before — Oscar, 2026-09-05).
        /// </summary>
        private float GenerateChallengeLevelUpSamples(float duration, int i)
        {
            float t = (float)i / SAMPLE_RATE;
            float[] notes = { 523f, 659f, 784f, 1047f, 1319f };
            float noteLength = duration / notes.Length;
            int noteIndex = Mathf.Min((int)(t / noteLength), notes.Length - 1);
            float noteT = (t - noteIndex * noteLength) / noteLength;
            float envelope = (1f - noteT * 0.35f) * (1f - t / duration * 0.4f);
            float freq = notes[noteIndex];
            float shimmer = Mathf.Sin(2f * Mathf.PI * freq * 3f * t) * 0.12f * (t / duration);
            return (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.6f
                  + Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * 0.25f + shimmer) * envelope * 0.75f;
        }

        public void PlayChallengeLevelUp()
        {
            PlayClip(_challengeLevelUpClip, 0.7f);
        }

        // ---- UI voice (2026-09-07) ----
        // The buttons were silent while gameplay sang — every factory-made button taps
        // (UIFactory wraps onClick), the shop speaks on purchase/equip/deny, fairies announce
        // themselves. Placeholder tones like the consumables; override slots for real audio.

        /// <summary>Soft low thump — every UGUI button press (wired in UIFactory). One plain
        /// bass tone, and quiet enough that the chip chain's drive never squares it
        /// (peak × CHIP_DRIVE stays under the clamp), so it keeps a round, unobtrusive body.</summary>
        private float GenerateUiTapSamples(float duration, int i)
        {
            float t = (float)i / SAMPLE_RATE;
            float freq = Mathf.Lerp(240f, 180f, t / duration);
            float envelope = 1f - (t / duration);
            return Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * envelope * 0.3f;
        }

        public void PlayUiTap() => PlayClip(_uiTapClip, 0.3f);

        /// <summary>Coin ka-ching: two quick high strikes with a sparkle tail — successful spends.</summary>
        private float GeneratePurchaseSamples(float duration, int i)
        {
            float t = (float)i / SAMPLE_RATE;
            float split = duration * 0.35f;
            float envelope;
            float signal;
            if (t < split)
            {
                float noteT = t / split;
                envelope = 1f - noteT * 0.3f;
                signal = Mathf.Sin(2f * Mathf.PI * 1568f * t) * 0.5f   // G6
                       + Mathf.Sin(2f * Mathf.PI * 2093f * t) * 0.25f; // C7
            }
            else
            {
                float t2 = (t - split) / (duration - split);
                envelope = 1f - t2;
                signal = Mathf.Sin(2f * Mathf.PI * 2093f * t) * 0.45f  // C7
                       + Mathf.Sin(2f * Mathf.PI * 3136f * t) * 0.2f;  // G7 sparkle
            }
            return signal * envelope * 0.7f;
        }

        public void PlayPurchase() => PlayClip(_purchaseClip, 0.6f);

        /// <summary>Quick two-note up-confirm — a skin equipping.</summary>
        private float GenerateEquipSamples(float duration, int i)
        {
            float t = (float)i / SAMPLE_RATE;
            float half = duration * 0.5f;
            float freq = t < half ? 660f : 880f; // E5 -> A5
            float noteT = (t < half ? t : t - half) / half;
            float envelope = (1f - noteT * 0.5f) * (1f - t / duration * 0.3f);
            return (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.6f
                  + Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * 0.2f) * envelope * 0.7f;
        }

        public void PlayEquip() => PlayClip(_equipClip, 0.55f);

        /// <summary>Low double buzz — a denied spend (pairs with the shake).</summary>
        private float GenerateDenySamples(float duration, int i)
        {
            float t = (float)i / SAMPLE_RATE;
            float gate = Mathf.Sin(2f * Mathf.PI * 14f * t) > 0f ? 1f : 0.25f; // two pulses
            float envelope = 1f - (t / duration) * 0.6f;
            float square = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 130f * t)) * 0.35f
                         + Mathf.Sin(2f * Mathf.PI * 98f * t) * 0.3f;
            return square * gate * envelope * 0.6f;
        }

        public void PlayDeny() => PlayClip(_denyClip, 0.5f);

        /// <summary>Rising flutter with vibrato — a Burger Fairy entering the screen.</summary>
        private float GenerateFairyAppearSamples(float duration, int i)
        {
            float t = (float)i / SAMPLE_RATE;
            float progress = t / duration;
            float baseFreq = Mathf.Lerp(740f, 1480f, progress);          // F#5 -> F#6 rise
            float vibrato = 1f + Mathf.Sin(2f * Mathf.PI * 9f * t) * 0.03f; // wing flutter
            float envelope = Mathf.Sin(progress * Mathf.PI);              // swell in and out
            return (Mathf.Sin(2f * Mathf.PI * baseFreq * vibrato * t) * 0.5f
                  + Mathf.Sin(2f * Mathf.PI * baseFreq * 2f * t) * 0.15f) * envelope * 0.6f;
        }

        public void PlayFairyAppear() => PlayClip(_fairyAppearClip, 0.45f);

        /// <summary>
        /// Descending tones for game over (A4, F#4, Eb4, C4)
        /// </summary>
        private float GenerateGameOverSamples(float duration, int i)
        {
            float t = (float)i / SAMPLE_RATE;
            float[] notes = { 440f, 370f, 311f, 262f };
            float noteLength = duration / notes.Length;
            int noteIndex = Mathf.Min((int)(t / noteLength), notes.Length - 1);
            float noteT = (t - noteIndex * noteLength) / noteLength;
            float envelope = (1f - noteT * 0.4f) * (1f - t / duration * 0.3f);
            float freq = notes[noteIndex];
            return (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.6f
                  + Mathf.Sin(2f * Mathf.PI * freq * 0.5f * t) * 0.4f) * envelope * 0.8f;
        }

        // ---- Consumables ----
        // Placeholder procedural tones wired to the hooks; the override fields let authored clips
        // drop in later (real sound design is the deferred audio pass).

        public void PlayConsumableCollect() => PlayClip(_consumableCollectClip, 0.6f);

        public void PlayConsumableUse(ConsumableType type)
        {
            AudioClip clip = type switch
            {
                ConsumableType.Ketchup => _consumableKetchupClip,
                ConsumableType.Mustard => _consumableMustardClip,
                _ => _consumableSkewerClip,
            };
            PlayClip(clip, 0.6f);
        }

        public void PlayConsumableFizzle() => PlayClip(_consumableFizzleClip, 0.5f);

        /// <summary>Bright two-note pickup for collecting a consumable (G5, D6).</summary>
        private float GenerateConsumableCollectSamples(float duration, int i)
        {
            float t = (float)i / SAMPLE_RATE;
            float[] notes = { 784f, 1175f };
            float noteLength = duration / notes.Length;
            int noteIndex = Mathf.Min((int)(t / noteLength), notes.Length - 1);
            float noteT = (t - noteIndex * noteLength) / noteLength;
            float envelope = 1f - noteT * 0.5f;
            return Mathf.Sin(2f * Mathf.PI * notes[noteIndex] * t) * envelope * 0.7f;
        }

        /// <summary>Ketchup: wet descending splat.</summary>
        private float GenerateKetchupSamples(float duration, int i)
        {
            float t = (float)i / SAMPLE_RATE;
            float freq = Mathf.Lerp(520f, 140f, t / duration);
            float envelope = 1f - t / duration;
            float wet = Mathf.Sin(2f * Mathf.PI * freq * 6f * t) * 0.15f;
            return (Mathf.Sin(2f * Mathf.PI * freq * t) + wet) * envelope * 0.6f;
        }

        /// <summary>Mustard: quick rising squirt.</summary>
        private float GenerateMustardSamples(float duration, int i)
        {
            float t = (float)i / SAMPLE_RATE;
            float freq = Mathf.Lerp(300f, 1300f, t / duration);
            float envelope = (1f - t / duration) * (1f - t / duration);
            return Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.6f;
        }

        /// <summary>Skewer: low thunk + slam.</summary>
        private float GenerateSkewerSamples(float duration, int i)
        {
            float t = (float)i / SAMPLE_RATE;
            float freq = Mathf.Lerp(180f, 90f, t / duration);
            float envelope = Mathf.Exp(-t / duration * 5f);
            return (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.7f
                  + Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * 0.3f) * envelope * 0.8f;
        }

        /// <summary>Fizzle: dull descending blip (missed / no target).</summary>
        private float GenerateFizzleSamples(float duration, int i)
        {
            float t = (float)i / SAMPLE_RATE;
            float freq = Mathf.Lerp(380f, 180f, t / duration);
            float envelope = 1f - t / duration;
            return Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.4f;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            GridManager gridManager = GridManager.Instance;
            if (gridManager != null)
            {
                gridManager.OnMatchEffect -= HandleMatch;
                gridManager.OnBurgerEffect -= HandleBurger;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnLevelChanged -= HandleLevelUp;
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
            }
        }
    }
}
