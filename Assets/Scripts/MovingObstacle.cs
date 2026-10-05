using UnityEngine;

/// <summary>
/// Obstacle that patrols between two points. Optionally lethal on touch.
/// </summary>
public class MovingObstacle : MonoBehaviour
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float speed = 2f;
    [SerializeField] private bool lethalOnTouch;
    [SerializeField] private Transform overrideRespawn;

    private void Update()
    {
        if (pointA == null || pointB == null) return;

        float t = Mathf.PingPong(Time.time * speed, 1f);
        transform.position = Vector3.Lerp(pointA.position, pointB.position, t);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!lethalOnTouch) return;
        HandleHit(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!lethalOnTouch) return;
        HandleHit(other);
    }

    private void HandleHit(Collider2D other)
    {
        if (!other.CompareTag("Player") && other.GetComponent<PlayerController>() == null)
            return;

        Transform spawn = overrideRespawn;
        if (spawn == null && ZoneManager.Instance != null)
            spawn = ZoneManager.Instance.GetCurrentSpawn();

        if (spawn != null)
        {
            other.transform.position = spawn.position;
            Rigidbody2D rb = other.attachedRigidbody;
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        SpikeHazard spike = GetComponent<SpikeHazard>();
        if (spike == null)
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    private void OnDrawGizmosSelected()
    {
        if (pointA == null || pointB == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(pointA.position, pointB.position);
        Gizmos.DrawWireSphere(pointA.position, 0.15f);
        Gizmos.DrawWireSphere(pointB.position, 0.15f);
    }
}
