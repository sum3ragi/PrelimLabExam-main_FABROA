using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Persistent playthrough state: coins, opened chests, and unlocked gates.
/// Survives scene reloads via DontDestroyOnLoad.
/// </summary>
public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    [SerializeField] private int startingCoins;

    private readonly HashSet<string> openedChests = new HashSet<string>();
    private readonly HashSet<string> unlockedGates = new HashSet<string>();

    public int Coins { get; private set; }

    public event System.Action<int> CoinsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Coins = startingCoins;
    }

    public void AddCoins(int amount)
    {
        if (amount == 0) return;
        Coins = Mathf.Max(0, Coins + amount);
        CoinsChanged?.Invoke(Coins);
    }

    public bool TrySpend(int cost)
    {
        if (cost <= 0) return true;
        if (Coins < cost) return false;

        Coins -= cost;
        CoinsChanged?.Invoke(Coins);
        return true;
    }

    public bool IsChestOpened(string id) =>
        !string.IsNullOrEmpty(id) && openedChests.Contains(id);

    public void MarkChestOpened(string id)
    {
        if (!string.IsNullOrEmpty(id))
            openedChests.Add(id);
    }

    public bool IsGateUnlocked(string id) =>
        !string.IsNullOrEmpty(id) && unlockedGates.Contains(id);

    public void UnlockGate(string id)
    {
        if (!string.IsNullOrEmpty(id))
            unlockedGates.Add(id);
    }

    public void ResetProgress()
    {
        Coins = startingCoins;
        openedChests.Clear();
        unlockedGates.Clear();
        CoinsChanged?.Invoke(Coins);
    }
}
