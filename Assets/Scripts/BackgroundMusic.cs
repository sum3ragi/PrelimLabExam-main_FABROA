using UnityEngine;

/// <summary>
/// Manages background music playback.
/// Automatically sets up an AudioSource to loop and plays background music when Play Mode starts.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("The audio clip to play as background music.")]
    [SerializeField] private AudioClip musicClip;

    [Tooltip("Volume of the background music (0.0 to 1.0).")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.6f;

    [Tooltip("Whether the music should automatically loop.")]
    [SerializeField] private bool loop = true;

    [Tooltip("Play music immediately when scene loads.")]
    [SerializeField] private bool playOnAwake = true;

    private AudioSource audioSource;

    public AudioSource AudioSource => audioSource;
    public bool IsPlaying => audioSource != null && audioSource.isPlaying;

    private void Awake()
    {
        InitializeAudioSource();
    }

    private void Start()
    {
        if (playOnAwake && !audioSource.isPlaying)
        {
            PlayMusic();
        }
    }

    /// <summary>
    /// Ensures AudioSource component is properly configured for looping background music.
    /// </summary>
    public void InitializeAudioSource()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        audioSource.loop = loop;
        audioSource.volume = volume;
        audioSource.playOnAwake = playOnAwake;

        // If no clip is assigned directly, try loading from Assets/Audio/BackgroundMusic
        if (musicClip == null && audioSource.clip != null)
        {
            musicClip = audioSource.clip;
        }

        if (musicClip == null)
        {
            musicClip = Resources.Load<AudioClip>("BackgroundMusic");
        }

        if (musicClip != null)
        {
            audioSource.clip = musicClip;
        }
    }

    /// <summary>
    /// Plays or resumes the background music.
    /// </summary>
    public void PlayMusic()
    {
        if (audioSource == null) InitializeAudioSource();

        if (audioSource.clip != null)
        {
            audioSource.Play();
            Debug.Log($"[BackgroundMusic] Playing looping music: {audioSource.clip.name}");
        }
        else
        {
            Debug.LogWarning("[BackgroundMusic] No audio clip assigned to AudioSource.");
        }
    }

    /// <summary>
    /// Stops the background music.
    /// </summary>
    public void StopMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    /// <summary>
    /// Sets music volume dynamically.
    /// </summary>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    private void OnValidate()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.loop = loop;
            audioSource.volume = volume;
            if (musicClip != null) audioSource.clip = musicClip;
        }
    }
}
