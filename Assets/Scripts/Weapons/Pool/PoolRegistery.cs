using System.Collections.Generic;
using UnityEngine;

public class PoolRegistry : MonoBehaviour
{
    public static PoolRegistry Instance { get; private set; }

    [System.Serializable]
    public class Entry
    {
        public ProjectileId id;
        public ProjectilePool pool;
    }

    [SerializeField] private List<Entry> pools = new();

    private Dictionary<ProjectileId, ProjectilePool> _map;

    private void Awake()
    {
        // simple singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _map = new Dictionary<ProjectileId, ProjectilePool>();

        foreach (var e in pools)
        {
            if (e.pool == null) continue;

            if (_map.ContainsKey(e.id))
            {
                Debug.LogWarning($"PoolRegistry: Duplicate pool entry for {e.id} on {name}. Using last one.");
            }

            _map[e.id] = e.pool;
        }
    }

    public ProjectilePool GetPool(ProjectileId id)
    {
        if (_map != null && _map.TryGetValue(id, out var pool) && pool != null)
            return pool;

        Debug.LogWarning($"PoolRegistry: No pool registered for {id}.");
        return null;
    }
}