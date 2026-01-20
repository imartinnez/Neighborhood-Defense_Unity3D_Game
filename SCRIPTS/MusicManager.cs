using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Handles background music across the entire game:
/// - Persists between scenes (singleton)
/// - Plays menu music in Main Menu
/// - Plays a randomized playlist during gameplay
/// - Smooth fade-in / fade-out transitions
/// - Uses unscaled time (independent of Time.timeScale)
public class MusicManager : MonoBehaviour
{
    // Singleton instance
    public static MusicManager Instance { get; private set; }

    // Music clips
    [Header("Clips")]
    [SerializeField] AudioClip menuMusic;       // Looping menu music
    [SerializeField] AudioClip[] gamePlaylist;  // Gameplay playlist (3–6 clips recommended)

    // Audio settings
    [Header("Settings")]
    [SerializeField, Range(0f, 1f)] float volume = 0.1f; // Target music volume
    [SerializeField] float fadeSeconds = 0.6f;           // Fade duration
    [SerializeField] bool randomStartTime = true;        // Start clips at random time

    AudioSource src;
    int lastIndex = -1;
    Coroutine playlistCo;

    // Initialization
    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);

        // AudioSource setup
        src = GetComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = false;
        src.spatialBlend = 0f; // 2D audio
        src.volume = 0f;

        // Listen for scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Scene-based music switching
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            StartMenuMusic();
        }
        else if (scene.name == "Suburb_Street")
        {
            StartGameMusic();
        }
    }

    // --- MENU MUSIC ---

    void StartMenuMusic()
    {
        StopPlaylist();
        StartCoroutine(FadeTo(menuMusic, loop: true));
    }

    // --- GAME MUSIC ---

    void StartGameMusic()
    {
        StopPlaylist();
        playlistCo = StartCoroutine(GamePlaylistRoutine());
    }

    /// Plays the gameplay playlist indefinitely with fades and random order.
    IEnumerator GamePlaylistRoutine()
    {
        // Fade out any currently playing music
        yield return FadeOut();

        while (true)
        {
            if (gamePlaylist == null || gamePlaylist.Length == 0)
                yield break;

            int idx = PickNextIndex();
            AudioClip clip = gamePlaylist[idx];

            src.clip = clip;
            src.loop = false;

            // Optionally start at a random point in the clip
            if (randomStartTime && clip.length > 10f)
                src.time = Random.Range(0f, clip.length - 5f);

            src.Play();
            yield return FadeIn();

            // Wait until the clip finishes (real time, not affected by timeScale)
            while (src.isPlaying)
                yield return null;

            // Small pause between tracks to avoid abrupt transitions
            yield return new WaitForSecondsRealtime(0.15f);
        }
    }

    /// Picks a random track index different from the last one.
    int PickNextIndex()
    {
        if (gamePlaylist.Length == 1)
            return 0;

        int idx;
        do
        {
            idx = Random.Range(0, gamePlaylist.Length);
        }
        while (idx == lastIndex);

        lastIndex = idx;
        return idx;
    }

    /// Stops the current playlist coroutine if running.
    void StopPlaylist()
    {
        if (playlistCo != null)
        {
            StopCoroutine(playlistCo);
            playlistCo = null;
        }
    }

    /// Fades out current music and fades in a new clip.
    IEnumerator FadeTo(AudioClip newClip, bool loop)
    {
        yield return FadeOut();

        src.clip = newClip;
        src.loop = loop;

        if (newClip != null)
        {
            if (randomStartTime && !loop && newClip.length > 10f)
                src.time = Random.Range(0f, newClip.length - 5f);

            src.Play();
            yield return FadeIn();
        }
    }

    /// Smoothly fades out the current track.
    IEnumerator FadeOut()
    {
        float t = 0f;
        float start = src.volume;

        while (t < fadeSeconds)
        {
            t += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(start, 0f, t / fadeSeconds);
            yield return null;
        }

        src.volume = 0f;
        src.Stop();
    }

    /// Smoothly fades in the current track to target volume.
    IEnumerator FadeIn()
    {
        float t = 0f;

        while (t < fadeSeconds)
        {
            t += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(0f, volume, t / fadeSeconds);
            yield return null;
        }

        src.volume = volume;
    }
}