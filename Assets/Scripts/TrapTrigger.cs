using UnityEngine;

public class TrapTrigger : MonoBehaviour
{
    [SerializeField] private GameObject trap; // Reference to your hidden trap

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object entering the trigger is the player
        if (other.CompareTag("Player"))
        {
            if (trap != null)
            {
                trap.SetActive(true); // Unhides the trap
            }
        }
    }
}