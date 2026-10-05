using UnityEngine;

/// <summary>
/// Attached to hazard/obstacle GameObjects in the scene.
/// Functions as a physical barrier and hazard that players must jump over.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Hazard : MonoBehaviour
{
    [Header("Hazard Settings")]
    [Tooltip("Amount of damage or penalty when touched.")]
    [SerializeField] private int damageAmount = 1;

    [Tooltip("Should this obstacle apply a knockback bounce to the player on contact?")]
    [SerializeField] private bool applyKnockback = true;

    [Tooltip("Knockback force applied if player hits the hazard from the side.")]
    [SerializeField] private float knockbackForce = 5f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if player collided with the hazard
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.GetComponent<PlayerController>() != null)
        {
            // Check if player is landing on or standing on top of the crate
            bool isTopContact = false;
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // Normal points towards the player; if normal.y > 0.5f or contact is on the top surface, player is on top
                if (contact.normal.y > 0.5f || contact.point.y >= transform.position.y + 0.45f)
                {
                    isTopContact = true;
                    break;
                }
            }

            // If landing on top of the crate, treat it as solid walkable ground with zero knockback
            if (isTopContact)
            {
                return;
            }

            Debug.Log($"[Hazard] Player collided with side of obstacle: {gameObject.name}");

            if (applyKnockback)
            {
                Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    // Push player backward horizontally away from hazard
                    float pushDirX = collision.transform.position.x > transform.position.x ? 1f : -1f;
                    playerRb.linearVelocity = new Vector2(pushDirX * knockbackForce, playerRb.linearVelocity.y);
                }
            }
        }
    }
}
