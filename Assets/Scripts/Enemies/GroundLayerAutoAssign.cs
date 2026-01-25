using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Attach to Player/Enemy root. Pulls tilemap references from LevelGroundProvider (scene object)
/// and assigns the correct layer based on current world position.
/// </summary>
public class GroundLayerAutoAssign : MonoBehaviour
{
    [Header("Layers (object will be set to one of these)")]
    [SerializeField] private string highLayerName = "Player_High";
    [SerializeField] private string lowLayerName  = "Player_Low";

    [Header("Sampling")]
    [Tooltip("Offset from transform.position to sample (usually feet).")]
    [SerializeField] private Vector2 sampleOffset = Vector2.zero;

    [Tooltip("If true, will assign once on Start(). Otherwise call EvaluateAndApplyLayer() manually.")]
    [SerializeField] private bool assignOnStart = true;

    [Tooltip("If the sample is ambiguous (both/neither), scan down a little to find a tile.")]
    [SerializeField] private bool useDownScanFallback = true;

    [Tooltip("Down scan step size in world units.")]
    [SerializeField] private float downScanStep = 0.1f;

    [Tooltip("How many steps to scan down (total distance = step * count).")]
    [SerializeField] private int downScanSteps = 6;

    private int _highLayer;
    private int _lowLayer;

    private Tilemap _highGroundMap;
    private Tilemap _lowGroundMap;

    private void Awake()
    {
        _highLayer = LayerMask.NameToLayer(highLayerName);
        _lowLayer  = LayerMask.NameToLayer(lowLayerName);

        if (_highLayer < 0 || _lowLayer < 0)
            Debug.LogError($"GroundLayerAutoAssign: Missing layer(s) '{highLayerName}'/'{lowLayerName}'. Check Tags & Layers.");
    }

    private void Start()
    {
        ResolveMapsFromProvider();

        if (assignOnStart)
            EvaluateAndApplyLayer();
    }

    /// <summary>
    /// Call this "on demand" (e.g., from ShiftPlayerLayer) to force the correct current layer.
    /// Returns the layer that was applied (or current layer if undecidable/missing refs).
    /// </summary>
    public int EvaluateAndApplyLayer()
    {
        int target = EvaluateLayerOnly();
        if (target <= 0) return gameObject.layer;

        SetLayerRecursively(gameObject, target);
        return target;
    }

    /// <summary>
    /// Returns which layer should be used at current position without changing anything.
    /// Returns -1 if undecidable.
    /// </summary>
    public int EvaluateLayerOnly()
    {
        if (!EnsureMaps())
            return -1;

        Vector3 sampleWorld = (Vector3)((Vector2)transform.position + sampleOffset);

        int decided = DecideLayerAtWorld(sampleWorld);
        if (decided > 0) return decided;

        if (!useDownScanFallback)
            return -1;

        // Optional: scan down a bit (helps when pivot isn't at feet)
        for (int i = 1; i <= Mathf.Max(0, downScanSteps); i++)
        {
            Vector3 p = sampleWorld + Vector3.down * (downScanStep * i);
            decided = DecideLayerAtWorld(p);
            if (decided > 0) return decided;
        }

        return -1;
    }

    // -------------------------------------------------
    // Internals
    // -------------------------------------------------

    private void ResolveMapsFromProvider()
    {
        var provider = LevelGroundProvider.Instance;
        if (provider == null)
        {
            Debug.LogError("GroundLayerAutoAssign: No LevelGroundProvider found in scene.");
            return;
        }

        _highGroundMap = provider.highGround;
        _lowGroundMap  = provider.lowGround;

        if (_highGroundMap == null || _lowGroundMap == null)
            Debug.LogError("GroundLayerAutoAssign: LevelGroundProvider has missing tilemap references.");
    }

    private bool EnsureMaps()
    {
        if (_highGroundMap != null && _lowGroundMap != null)
            return true;

        ResolveMapsFromProvider();
        return _highGroundMap != null && _lowGroundMap != null;
    }

    private int DecideLayerAtWorld(Vector3 worldPos)
    {
        bool onHigh = HasTileAt(_highGroundMap, worldPos);
        bool onLow  = HasTileAt(_lowGroundMap, worldPos);

        // If both overlap, pick a rule. Here: HIGH wins.
        if (onHigh && !onLow) return _highLayer;
        if (onLow && !onHigh) return _lowLayer;
        if (onHigh && onLow)  return _highLayer;

        return -1;
    }

    private static bool HasTileAt(Tilemap map, Vector3 worldPos)
    {
        if (map == null) return false;
        Vector3Int cell = map.WorldToCell(worldPos);
        return map.HasTile(cell);
    }

    private static void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, newLayer);
    }
}











/*using UnityEngine;
using UnityEngine.Tilemaps;

public class GroundLayerAutoAssign : MonoBehaviour
{
    [Header("Ground maps (tile data, not colliders)")]
    private Tilemap _highGroundMap;
    private Tilemap _lowGroundMap;

    [Header("Layers")]
    [SerializeField] private string highLayerName = "Enemy_High";
    [SerializeField] private string lowLayerName  = "Enemy_Low";

    [Header("Sampling")]
    [SerializeField] private Vector2 sampleOffset = Vector2.zero; // e.g. foot position offset

    private int _highLayer;
    private int _lowLayer;

    private void Awake()
    {
        _highLayer = LayerMask.NameToLayer(highLayerName);
        _lowLayer  = LayerMask.NameToLayer(lowLayerName);

        if (_highLayer < 0 || _lowLayer < 0)
            Debug.LogError("Enemy layers missing. Check Tags & Layers.");
    }

    private void Start()
    {
        var provider = LevelGroundProvider.Instance;
        if (provider == null)
        {
            Debug.LogError("No LevelGroundProvider in scene.");
            return;
        }

        _highGroundMap = provider.highGround;
        _lowGroundMap  = provider.lowGround;

        AssignOnce();
    }

    public void AssignOnce()
    {
        if (_highGroundMap == null || _lowGroundMap == null)
        {
            Debug.LogError("GroundLayerAutoAssign: Missing tilemap references.");
            return;
        }

        Vector3 world = (Vector3)((Vector2)transform.position + sampleOffset);

        // Prefer the map you want to “win” if both overlap
        bool onHigh = HasTileAt(_highGroundMap, world);
        //bool onLow  = HasTileAt(_lowGroundMap, world);

        if (onHigh)
        {
            SetLayerRecursively(gameObject, _highLayer);
            return;
        }

        if (!onHigh)
        {
            SetLayerRecursively(gameObject, _lowLayer);
            return;
        }

        // If ambiguous or on neither, pick a safe default (or keep current)
        // You can also do a small downward raycast to find ground then sample that position.
        //Debug.LogWarning($"{name}: Could not classify ground (onHigh={onHigh}, onLow={onLow}). Keeping layer.");
    }

    private static bool HasTileAt(Tilemap map, Vector3 worldPos)
    {
        Vector3Int cell = map.WorldToCell(worldPos);
        return map.HasTile(cell);
    }

    private static void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, newLayer);
    }
}*/
