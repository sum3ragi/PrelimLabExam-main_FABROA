using UnityEngine;

/// <summary>
/// Priced chest with a floating cost label. Early chests cost less; deep chests cost more.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PricedChest : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string chestId = "Chest_Forest_01";

    [Header("Economy")]
    [SerializeField] private int openCost = 20;
    [SerializeField] private LootTable loot;

    [Header("Visuals")]
    [SerializeField] private TextMesh costLabel;
    [SerializeField] private SpriteRenderer chestRenderer;
    [SerializeField] private Sprite openSprite;
    [SerializeField] private Color affordableColor = Color.yellow;
    [SerializeField] private Color unaffordableColor = new Color(1f, 0.4f, 0.4f);

    private bool opened;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Start()
    {
        EnsureCostLabel();
        RefreshLabel();

        if (GameSession.Instance != null && GameSession.Instance.IsChestOpened(chestId))
            ApplyOpenedVisual(skipLoot: true);
    }

    private void Update()
    {
        if (opened || costLabel == null || GameSession.Instance == null) return;
        costLabel.color = GameSession.Instance.Coins >= openCost ? affordableColor : unaffordableColor;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (opened || !IsPlayer(other)) return;
        TryOpen(other.gameObject);
    }

    public void TryOpen(GameObject player)
    {
        if (opened) return;

        if (GameSession.Instance == null)
        {
            Debug.LogWarning("[PricedChest] No GameSession in scene.");
            return;
        }

        if (GameSession.Instance.IsChestOpened(chestId))
        {
            ApplyOpenedVisual(skipLoot: true);
            return;
        }

        if (!GameSession.Instance.TrySpend(openCost))
        {
            Debug.Log($"[PricedChest] Need ${openCost}. Have ${GameSession.Instance.Coins}.");
            if (costLabel != null) costLabel.color = unaffordableColor;
            return;
        }

        GameSession.Instance.MarkChestOpened(chestId);
        ApplyOpenedVisual(skipLoot: false);
        loot?.Grant(player);
        Debug.Log($"[PricedChest] Opened '{chestId}' for ${openCost}.");
    }

    private void ApplyOpenedVisual(bool skipLoot)
    {
        opened = true;

        if (costLabel != null)
            costLabel.gameObject.SetActive(false);

        if (chestRenderer != null && openSprite != null)
            chestRenderer.sprite = openSprite;
    }

    private void EnsureCostLabel()
    {
        if (costLabel != null) return;

        Transform existing = transform.Find("CostLabel");
        if (existing != null)
        {
            costLabel = existing.GetComponent<TextMesh>();
            if (costLabel != null) return;
        }

        GameObject labelObj = new GameObject("CostLabel");
        labelObj.transform.SetParent(transform);
        labelObj.transform.localPosition = new Vector3(0f, 1.2f, 0f);

        costLabel = labelObj.AddComponent<TextMesh>();
        costLabel.anchor = TextAnchor.MiddleCenter;
        costLabel.alignment = TextAlignment.Center;
        costLabel.characterSize = 0.15f;
        costLabel.fontSize = 48;
        costLabel.color = affordableColor;
    }

    private void RefreshLabel()
    {
        if (costLabel != null)
            costLabel.text = $"${openCost}";
    }

    private void OnValidate()
    {
        if (costLabel != null)
            costLabel.text = $"${openCost}";
    }

    private static bool IsPlayer(Collider2D other)
    {
        return other.CompareTag("Player") || other.GetComponent<PlayerController>() != null;
    }
}
