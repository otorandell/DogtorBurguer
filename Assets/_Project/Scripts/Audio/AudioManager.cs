using System;
using UnityEngine;

namespace DogtorBurguer
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource _sfxSource;
        private AudioSource _squeezSource;

        private AudioClip _matchClip;
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

        private const int SAMPLE_RATE = 44100;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

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
            if (gridManager == null)
                gridManager = FindAnyObjectByType<GridManager>();

            if (gridManager != null)
            {
                gridManager.OnMatchEffect += HandleMatch;
                gridManager.OnBurgerEffect += HandleBurger;
            }

            DifficultyManager dm = FindAnyObjectByType<DifficultyManager>();
            if (dm != null)
                dm.OnLevelChanged += HandleLevelUp;

            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged += HandleStateChanged;
        }

        private void HandleMatch(Vector3 pos, int points)
        {
            PlayClip(_matchClip, 0.5f);
        }

        private void HandleBurger(Vector3 pos, int points, string name, int ingredientCount)
        {
            AudioClip clip;
            if (ingredientCount >= 9) clip = _burgerMaxClip;
            else if (ingredientCount >= 7) clip = _burgerMegaClip;
            else if (ingredientCount >= 5) clip = _burgerLargeClip;
            else if (ingredientCount >= 3) clip = _burgerMediumClip;
            else clip = _burgerSmallClip;

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
            _matchClip = GenerateSound("Match", 0.15f, GenerateMatchSamples);
            _burgerSmallClip = GenerateSound("BurgerSmall", 0.25f, GenerateBurgerSmallSamples);
            _burgerMediumClip = GenerateSound("BurgerMedium", 0.4f, GenerateBurgerMediumSamples);
            _burgerLargeClip = GenerateSound("BurgerLarge", 0.5f, GenerateBurgerLargeSamples);
            _burgerMegaClip = GenerateSound("BurgerMega", 0.6f, GenerateBurgerMegaSamples);
            _burgerMaxClip = GenerateSound("BurgerMax", 0.8f, GenerateBurgerMaxSamples);
            _levelUpClip = GenerateSound("LevelUp", 0.5f, GenerateLevelUpSamples);
            _gameOverClip = GenerateSound("GameOver", 0.8f, GenerateGameOverSamples);
            _squeezeClip = GenerateSound("Squeeze", 0.1f, GenerateSqueezeSamples);
            _fastDropClip = GenerateSound("FastDrop", 0.12f, GenerateFastDropSamples);
            _earlySpawnClip = GenerateSound("EarlySpawn", 0.15f, GenerateEarlySpawnSamples);
        }

        private AudioClip GenerateSound(string name, float duration, Func<float, int, float> sampleFunc)
        {
            int sampleCount = (int)(SAMPLE_RATE * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                samples[i] = sampleFunc(duration, i);
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

        private void OnDestroy()
        {
            GridManager gridManager = GridManager.Instance;
            if (gridManager == null)
                gridManager = FindAnyObjectByType<GridManager>();

            if (gridManager != null)
            {
                gridManager.OnMatchEffect -= HandleMatch;
                gridManager.OnBurgerEffect -= HandleBurger;
            }

            DifficultyManager dm = FindAnyObjectByType<DifficultyManager>();
            if (dm != null)
                dm.OnLevelChanged -= HandleLevelUp;

            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
        }
    }
}
