using UnityEngine;

/// <summary>
/// ScriptableObject defining what a priced chest grants on open.
/// Create via Assets → Create → Game → Loot Table.
/// </summary>
[CreateAssetMenu(menuName = "Game/Loot Table", fileName = "LootTable")]
public class LootTable : ScriptableObject
{
    [Header("Currency")]
    [Tooltip("Coins granted back when the chest opens (can be 0).")]
    public int coinGrant;

    [Header("Power-ups")]
    public bool grantJumpBoost;
    public float jumpBoostAmount = 2f;

    public bool grantMoveSpeedBoost;
    public float moveSpeedBoostAmount = 1f;

    [Tooltip("One-shot upward pop when opening a major chest.")]
    public bool grantJumpImpulse;
    public float jumpImpulseForce = 8f;

    [Header("Flavor")]
    [TextArea]
    public string description = "Basic loot";

    public void Grant(GameObject player)
    {
        if (player == null) return;

        if (coinGrant > 0 && GameSession.Instance != null)
            GameSession.Instance.AddCoins(coinGrant);

        PlayerPowerUps powerUps = player.GetComponent<PlayerPowerUps>();
        if (powerUps == null)
            powerUps = player.AddComponent<PlayerPowerUps>();

        if (grantJumpBoost)
            powerUps.AddJumpBoost(jumpBoostAmount);

        if (grantMoveSpeedBoost)
            powerUps.AddMoveSpeedBoost(moveSpeedBoostAmount);

        if (grantJumpImpulse)
            powerUps.ApplyJumpImpulse(jumpImpulseForce);

        Debug.Log($"[LootTable] Granted '{name}' ({description}) to {player.name}");
    }
}
