using UnityEngine;

/// <summary>
/// Locked gate / toll door. Blocks progress until coins are spent and/or a chest is opened.
/// </summary>
public class TollGate : MonoBehaviour
{
    public enum UnlockMode
    {
        SpendCoins,
        RequireChestOpened,
        Both
    }

    [Header("Identity")]
    [SerializeField] private string gateId = "ForestToRuins";

    [Header("Requirements")]
    [SerializeField] private UnlockMode mode = UnlockMode.SpendCoins;
    [SerializeField] private int coinCost = 50;
    [SerializeField] private string requiredChestId;

    [Header("Visuals / Collision")]
    [SerializeField] private Collider2D blockingCollider;
    [SerializeField] private GameObject closedVisual;
    [SerializeField] private GameObject openVisual;
    [SerializeField] private TextMesh costLabel;

    [Header("Interaction")]
    [SerializeField] private bool unlockOnTriggerEnter = true;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private bool unlocked;
    private bool playerInRange;

    private void Start()
    {
        RefreshCostLabel();

        if (GameSession.Instance != null && GameSession.Instance.IsGateUnlocked(gateId))
            SetOpen(true);
        else
            SetOpen(false);
    }

    private void Update()
    {
        if (unlocked || !playerInRange) return;
        if (!unlockOnTriggerEnter && Input.GetKeyDown(interactKey))
            TryUnlock();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;
        playerInRange = true;

        if (unlockOnTriggerEnter)
            TryUnlock();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsPlayer(other))
            playerInRange = false;
    }

    public void TryUnlock()
    {
        if (unlocked) return;
        if (GameSession.Instance == null)
        {
            Debug.LogWarning("[TollGate] No GameSession in scene.");
            return;
        }

        if (mode == UnlockMode.RequireChestOpened || mode == UnlockMode.Both)
        {
            if (!GameSession.Instance.IsChestOpened(requiredChestId))
            {
                Debug.Log($"[TollGate] '{gateId}' needs chest '{requiredChestId}' opened first.");
                return;
            }
        }

        if (mode == UnlockMode.SpendCoins || mode == UnlockMode.Both)
        {
            if (!GameSession.Instance.TrySpend(coinCost))
            {
                Debug.Log($"[TollGate] '{gateId}' needs ${coinCost}. Have ${GameSession.Instance.Coins}.");
                return;
            }
        }

        GameSession.Instance.UnlockGate(gateId);
        SetOpen(true);
        Debug.Log($"[TollGate] Unlocked '{gateId}'.");
    }

    private void SetOpen(bool open)
    {
        unlocked = open;

        if (blockingCollider != null)
            blockingCollider.enabled = !open;

        if (closedVisual != null)
            closedVisual.SetActive(!open);

        if (openVisual != null)
            openVisual.SetActive(open);

        if (costLabel != null)
            costLabel.gameObject.SetActive(!open);
    }

    private void RefreshCostLabel()
    {
        if (costLabel == null) return;

        if (mode == UnlockMode.RequireChestOpened)
            costLabel.text = "KEY";
        else
            costLabel.text = $"${coinCost}";
    }

    private static bool IsPlayer(Collider2D other)
    {
        return other.CompareTag("Player") || other.GetComponent<PlayerController>() != null;
    }
}
