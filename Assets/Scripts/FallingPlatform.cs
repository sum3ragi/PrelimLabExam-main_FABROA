using System.Collections;
using UnityEngine;

/// <summary>
/// Platform that shakes then falls when the player stands on it.
/// Keep on the Ground layer so PlayerController ground checks still work.
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class FallingPlatform : MonoBehaviour
{
    [SerializeField] private float shakeDelay = 0.35f;
    [SerializeField] private float shakeIntensity = 0.08f;
    [SerializeField] private float fallGravityScale = 2.5f;
    [SerializeField] private float resetAfterSeconds = 4f;
    [SerializeField] private bool respawnPlatform = true;

    private Rigidbody2D rb;
    private Vector3 startPos;
    private Quaternion startRot;
    private bool triggered;
    private Coroutine routine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        startPos = transform.position;
        startRot = transform.rotation;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (triggered) return;
        if (!collision.collider.CompareTag("Player") &&
            collision.collider.GetComponent<PlayerController>() == null)
            return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                triggered = true;
                routine = StartCoroutine(FallSequence());
                return;
            }
        }
    }

    private IEnumerator FallSequence()
    {
        float elapsed = 0f;
        while (elapsed < shakeDelay)
        {
            transform.position = startPos + (Vector3)(Random.insideUnitCircle * shakeIntensity);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = startPos;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = fallGravityScale;

        if (!respawnPlatform || resetAfterSeconds <= 0f)
            yield break;

        yield return new WaitForSeconds(resetAfterSeconds);
        ResetPlatform();
    }

    private void ResetPlatform()
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;
        triggered = false;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        transform.SetPositionAndRotation(startPos, startRot);
    }
}
