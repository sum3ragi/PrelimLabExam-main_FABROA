using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Lethal spike / pit hazard. Prefers zone respawn via ZoneManager; falls back to scene reload.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SpikeHazard : MonoBehaviour
{
    [SerializeField] private bool useZoneRespawn = true;
    [SerializeField] private Transform overrideRespawn;
    [SerializeField] private bool reloadSceneIfNoSpawn = true;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;
        KillOrRespawn(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsPlayer(collision.collider)) return;
        KillOrRespawn(collision.collider);
    }

    private void KillOrRespawn(Collider2D playerCol)
    {
        Transform spawn = overrideRespawn;
        if (spawn == null && useZoneRespawn && ZoneManager.Instance != null)
            spawn = ZoneManager.Instance.GetCurrentSpawn();

        if (spawn != null)
        {
            playerCol.transform.position = spawn.position;
            Rigidbody2D rb = playerCol.attachedRigidbody;
            if (rb != null) rb.linearVelocity = Vector2.zero;
            Debug.Log($"[SpikeHazard] Respawned player at {spawn.name}");
            return;
        }

        if (reloadSceneIfNoSpawn)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private static bool IsPlayer(Collider2D other)
    {
        return other.CompareTag("Player") || other.GetComponent<PlayerController>() != null;
    }
}
