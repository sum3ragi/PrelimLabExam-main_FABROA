using UnityEngine;

/// <summary>
/// Tracks the active zone id and optional camera / spawn anchors for a continuous level.
/// </summary>
public class ZoneManager : MonoBehaviour
{
    public static ZoneManager Instance { get; private set; }

    [SerializeField] private string startingZoneId = "Forest";
    [SerializeField] private ZoneInfo[] zones;

    public string CurrentZoneId { get; private set; }

    public event System.Action<string> ZoneChanged;

    [System.Serializable]
    public class ZoneInfo
    {
        public string zoneId;
        public Transform spawnPoint;
        public Collider2D cameraBounds;
        [TextArea] public string displayName;
    }

    private void Awake()
    {
        Instance = this;
        CurrentZoneId = startingZoneId;
    }

    public ZoneInfo GetZone(string zoneId)
    {
        if (zones == null) return null;
        foreach (var z in zones)
        {
            if (z != null && z.zoneId == zoneId)
                return z;
        }
        return null;
    }

    public void SetZone(string zoneId, bool teleportPlayer = false)
    {
        if (string.IsNullOrEmpty(zoneId)) return;

        CurrentZoneId = zoneId;
        ZoneInfo info = GetZone(zoneId);

        if (teleportPlayer && info != null && info.spawnPoint != null)
        {
            PlayerController player = FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                player.transform.position = info.spawnPoint.position;
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;
            }
        }

        ZoneChanged?.Invoke(zoneId);
        Debug.Log($"[ZoneManager] Entered zone: {zoneId}");
    }

    public Transform GetCurrentSpawn()
    {
        ZoneInfo info = GetZone(CurrentZoneId);
        return info != null ? info.spawnPoint : null;
    }
}
