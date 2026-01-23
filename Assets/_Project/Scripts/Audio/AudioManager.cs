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
        private AudioClip _burgerClip;
        private AudioClip _levelUpClip;
        private AudioClip _gameOverClip;
        private AudioClip _squeezeClip;

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

        private void HandleBurger(Vector3 pos, int points, string name)
        {
            PlayClip(_burgerClip, 0.7f);
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
            _burgerClip = GenerateSound("Burger", 0.4f, GenerateBurgerSamples);
            _levelUpClip = GenerateSound("LevelUp", 0.5f, GenerateLevelUpSamples);
            _gameOverClip = GenerateSound("GameOver", 0.8f, GenerateGameOverSamples);
            _squeezeClip = GenerateSound("Squeeze", 0.1f, GenerateSqueezeSamples);
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
        /// Ascending arpeggio for burger completion (C5, E5, G5, C6)
        /// </summary>
        private float GenerateBurgerSamples(float duration, int i)
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
