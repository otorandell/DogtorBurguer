using UnityEngine;
using UnityEngine.SceneManagement;

namespace DogtorBurguer
{
    public class MusicManager : MonoBehaviour
    {
        public static MusicManager Instance { get; private set; }

        private AudioSource _source;
        private AudioClip _menuTrack;
        private AudioClip _gameTrack;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _source = gameObject.AddComponent<AudioSource>();
            _source.loop = true;
            _source.playOnAwake = false;
            _source.volume = 0.5f;

            _menuTrack = Resources.Load<AudioClip>("Music/MenuTrack");
            _gameTrack = Resources.Load<AudioClip>("Music/GameTrack");

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            PlayTrackForCurrentScene();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            PlayTrackForCurrentScene();
        }

        private void PlayTrackForCurrentScene()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            AudioClip target = sceneName == SceneLoader.SCENE_MAIN_MENU ? _menuTrack : _gameTrack;

            if (target == null)
            {
                Debug.LogWarning($"[MusicManager] No track found for scene '{sceneName}'");
                return;
            }

            // Don't restart if the same track is already playing
            if (_source.clip == target && _source.isPlaying)
                return;

            _source.clip = target;
            _source.Play();
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
