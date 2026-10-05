using UnityEngine;

/// <summary>
/// Attached to Crate3 to provide 3D spatial audio with smooth distance attenuation.
/// As the player approaches, the audio fades in and gets louder; as the player walks away, it fades out.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class CrateSpatialAudio : MonoBehaviour
{
    [Header("Spatial Audio Settings")]
    [Tooltip("Distance at which the audio reaches maximum volume.")]
    [SerializeField] private float minDistance = 1.5f;

    [Tooltip("Distance beyond which the audio is completely silent.")]
    [SerializeField] private float maxDistance = 7.0f;

    [Tooltip("Peak volume when within minDistance.")]
    [Range(0f, 1f)]
    [SerializeField] private float maxVolume = 0.9f;

    [Tooltip("Audio clip for ambient sound.")]
    [SerializeField] private AudioClip ambientClip;

    [Header("Target Tracking")]
    [Tooltip("Reference to the player Transform. Automatically resolved if null.")]
    [SerializeField] private Transform playerTransform;

    private AudioSource audioSource;

    public float MinDistance => minDistance;
    public float MaxDistance => maxDistance;
    public float CurrentDistance { get; private set; }
    public float CurrentVolume => audioSource != null ? audioSource.volume : 0f;

    private void Awake()
    {
        InitializeAudioSource();
        FindPlayer();
    }

    private void Start()
    {
        if (playerTransform == null)
        {
            FindPlayer();
        }

        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }

        UpdateAttenuation();
    }

    private void FindPlayer()
    {
        if (playerTransform != null) return;

        PlayerController pc = Object.FindFirstObjectByType<PlayerController>();
        if (pc != null)
        {
            playerTransform = pc.transform;
            return;
        }

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    public void InitializeAudioSource()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
        {
            // 3D Spatial Audio setup
            audioSource.spatialBlend = 1.0f; // 100% 3D
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = minDistance;
            audioSource.maxDistance = maxDistance;
            audioSource.loop = true;
            audioSource.playOnAwake = true;
            audioSource.dopplerLevel = 0f;

            if (ambientClip != null && audioSource.clip == null)
            {
                audioSource.clip = ambientClip;
            }
            else if (audioSource.clip != null && ambientClip == null)
            {
                ambientClip = audioSource.clip;
            }
        }
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            FindPlayer();
            if (playerTransform == null) return;
        }

        UpdateAttenuation();
    }

    /// <summary>
    /// Smoothly calculates and applies audio volume attenuation based on 2D player proximity.
    /// </summary>
    public void UpdateAttenuation()
    {
        if (audioSource == null || playerTransform == null) return;

        CurrentDistance = Vector2.Distance(transform.position, playerTransform.position);

        if (CurrentDistance <= minDistance)
        {
            audioSource.volume = maxVolume;
        }
        else if (CurrentDistance >= maxDistance)
        {
            audioSource.volume = 0f;
        }
        else
        {
            // Smoothly interpolate between minDistance and maxDistance
            float factor = 1f - ((CurrentDistance - minDistance) / (maxDistance - minDistance));
            // Smooth cosine / smoothstep curve for natural acoustic fade
            float smoothFactor = Mathf.SmoothStep(0f, 1f, factor);
            audioSource.volume = maxVolume * smoothFactor;
        }

        // Ensure playback remains active when in range
        if (audioSource.volume > 0.001f && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    private void OnValidate()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.spatialBlend = 1.0f;
            audioSource.minDistance = minDistance;
            audioSource.maxDistance = maxDistance;
            audioSource.loop = true;
            if (ambientClip != null) audioSource.clip = ambientClip;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize attenuation ranges in editor
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, minDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxDistance);
    }
}
