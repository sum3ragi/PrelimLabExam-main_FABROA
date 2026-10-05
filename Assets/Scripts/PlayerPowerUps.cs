using UnityEngine;

/// <summary>
/// Optional power-up modifiers applied by loot tables (jump boost, move speed).
/// Attach to the same GameObject as PlayerController.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerPowerUps : MonoBehaviour
{
    private float jumpBonus;
    private float moveSpeedBonus;

    public float GetJumpBonus() => jumpBonus;
    public float GetMoveSpeedBonus() => moveSpeedBonus;

    public void AddJumpBoost(float amount)
    {
        jumpBonus += amount;
        Debug.Log($"[PlayerPowerUps] Jump boost +{amount} (total bonus {jumpBonus})");
    }

    public void AddMoveSpeedBoost(float amount)
    {
        moveSpeedBonus += amount;
        Debug.Log($"[PlayerPowerUps] Move speed boost +{amount} (total bonus {moveSpeedBonus})");
    }

    /// <summary>
    /// Instant upward pop used by major chest loot.
    /// </summary>
    public void ApplyJumpImpulse(float extraForce)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null) return;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, 0f) + extraForce);
    }
}
