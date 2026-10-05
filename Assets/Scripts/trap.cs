using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Lethal trap. Prefers ZoneManager respawn; otherwise reloads the active scene.
/// </summary>
public class trap : MonoBehaviour
{
    [SerializeField] private bool useZoneRespawn = true;

    void OnCollisionEnter2D(Collision2D hit)
    {
        if (hit.collider.tag != "Player" && hit.collider.GetComponent<PlayerController>() == null)
            return;

        if (useZoneRespawn && ZoneManager.Instance != null)
        {
            Transform spawn = ZoneManager.Instance.GetCurrentSpawn();
            if (spawn != null)
            {
                hit.collider.transform.position = spawn.position;
                Rigidbody2D rb = hit.collider.attachedRigidbody;
                if (rb != null) rb.linearVelocity = Vector2.zero;
                return;
            }
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
