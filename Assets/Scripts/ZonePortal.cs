using UnityEngine;

/// <summary>
/// Connects platforming areas. Teleports the player to an exit point when entered,
/// optionally requiring a gate to be unlocked first.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ZonePortal : MonoBehaviour
{
    [SerializeField] private string targetZoneId = "Ruins";
    [SerializeField] private Transform exitPoint;
    [SerializeField] private string requiredGateId;
    [SerializeField] private bool teleportOnEnter = true;
    [SerializeField] private string lockedMessage = "Gate locked — open the toll door first.";

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;

        if (!string.IsNullOrEmpty(requiredGateId))
        {
            if (GameSession.Instance == null || !GameSession.Instance.IsGateUnlocked(requiredGateId))
            {
                Debug.Log($"[ZonePortal] {lockedMessage}");
                return;
            }
        }

        if (ZoneManager.Instance != null)
            ZoneManager.Instance.SetZone(targetZoneId, teleportPlayer: false);

        if (teleportOnEnter && exitPoint != null)
        {
            other.transform.position = exitPoint.position;
            Rigidbody2D rb = other.attachedRigidbody;
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
    }

    private static bool IsPlayer(Collider2D other)
    {
        return other.CompareTag("Player") || other.GetComponent<PlayerController>() != null;
    }
}
