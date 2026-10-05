using UnityEngine;

/// <summary>
/// Collectible coin that adds to GameSession currency.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CoinPickup : MonoBehaviour
{
    [SerializeField] private int value = 5;
    [SerializeField] private bool destroyOnPickup = true;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;
        if (GameSession.Instance == null)
        {
            Debug.LogWarning("[CoinPickup] No GameSession in scene.");
            return;
        }

        GameSession.Instance.AddCoins(value);

        if (destroyOnPickup)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }

    private static bool IsPlayer(Collider2D other)
    {
        return other.CompareTag("Player") || other.GetComponent<PlayerController>() != null;
    }
}
