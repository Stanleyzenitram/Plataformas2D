using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    [System.Serializable]
    public class SceneMusic
    {
        public string sceneName;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 0.5f;
    }

    public static MusicManager Instance { get; private set; }

    [SerializeField] private SceneMusic[] tracks;

    private AudioSource source;

    private void Awake()
    {
        // Solo debe existir un MusicManager entre escenas
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        source = GetComponent<AudioSource>();
        source.loop = true;
        source.playOnAwake = false;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // La primera escena ya está cargada cuando nace el manager
        PlayForScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayForScene(scene.name);
    }

    private void PlayForScene(string sceneName)
    {
        foreach (SceneMusic track in tracks)
        {
            if (track.sceneName != sceneName) continue;

            // Si ya suena esa canción, no la reinicia
            if (source.clip == track.clip && source.isPlaying) return;

            source.clip = track.clip;
            source.volume = track.volume;
            source.Play();
            return;
        }

        // Escena sin música asignada
        source.Stop();
        source.clip = null;
    }

    public void SetVolume(float volume)
    {
        source.volume = volume;
    }
}