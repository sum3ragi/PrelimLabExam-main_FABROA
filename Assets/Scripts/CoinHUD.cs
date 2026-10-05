using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple UI Text binder for GameSession coin count.
/// Assign a UI Text (ugui) in the inspector.
/// </summary>
public class CoinHUD : MonoBehaviour
{
    [SerializeField] private Text coinText;
    [SerializeField] private string prefix = "Coins: $";

    private void OnEnable()
    {
        TryBind();
        Refresh(GameSession.Instance != null ? GameSession.Instance.Coins : 0);
    }

    private void OnDisable()
    {
        if (GameSession.Instance != null)
            GameSession.Instance.CoinsChanged -= Refresh;
    }

    private void Start()
    {
        TryBind();
        Refresh(GameSession.Instance != null ? GameSession.Instance.Coins : 0);
    }

    private void TryBind()
    {
        if (GameSession.Instance == null) return;
        GameSession.Instance.CoinsChanged -= Refresh;
        GameSession.Instance.CoinsChanged += Refresh;
    }

    private void Refresh(int coins)
    {
        if (coinText != null)
            coinText.text = prefix + coins;
    }
}
