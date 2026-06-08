using UnityEngine;
using UnityEngine.SceneManagement;

namespace DogtorBurguer
{
    public class MusicManager : Singleton<MusicManager>
    {
        private AudioSource _source;
        private AudioClip[] _menuTracks;
        private AudioClip[] _gameTracks;
        private MusicCategory _currentCategory = MusicCategory.None;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) return;
            DontDestroyOnLoad(gameObject);

            _source = gameObject.AddComponent<AudioSource>();
            _source.loop = true;
            _source.playOnAwake = false;
            _source.volume = AudioConfig.DEFAULT_MUSIC_VOLUME;

            _menuTracks = Resources.LoadAll<AudioClip>("Music/MenuTrack");
            _gameTracks = Resources.LoadAll<AudioClip>("Music/GameTrack");

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            PlayTrackForCurrentScene();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            PlayTrackForCurrentScene();
        }

        private void PlayTrackForCurrentScene()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            bool isMenu = sceneName == SceneLoader.SCENE_MAIN_MENU;
            MusicCategory target = isMenu ? MusicCategory.Menu : MusicCategory.Game;
            AudioClip[] tracks = isMenu ? _menuTracks : _gameTracks;

            if (tracks == null || tracks.Length == 0)
            {
                Debug.LogWarning($"[MusicManager] No tracks found in Resources/Music/{(isMenu ? "MenuTrack" : "GameTrack")}/");
                return;
            }

            // Don't restart if already playing from the same category
            if (_currentCategory == target && _source.isPlaying)
                return;

            // Pick a random track
            AudioClip clip = tracks[Rng.Range(0, tracks.Length)];
            _source.clip = clip;
            _source.Play();

            _currentCategory = target;
        }

        public void SetVolume(float volume)
        {
            _source.volume = volume;
        }

        public void ApplySoundSetting()
        {
            bool soundOn = SaveDataManager.Instance != null ? SaveDataManager.Instance.SoundOn : true;
            _source.mute = !soundOn;
        }
    }
}
