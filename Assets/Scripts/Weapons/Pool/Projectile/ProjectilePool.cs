using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    [SerializeField] private PooledProjectile projectilePrefab;
    [SerializeField] private int prewarmCount = 50;
    [SerializeField] private bool allowGrowth = true;

    private readonly Queue<PooledProjectile> _pool = new();

    private void Awake()
    {
        Prewarm();
    }

    private void Prewarm()
    {
        if (projectilePrefab == null) return;

        for (int i = 0; i < prewarmCount; i++)
        {
            var p = CreateInstance();
            Return(p);
        }
    }

    private PooledProjectile CreateInstance()
    {
        var p = Instantiate(projectilePrefab, transform);
        p.AssignPool(this);
        p.gameObject.SetActive(false);
        return p;
    }

    public PooledProjectile Get(Vector3 position, Quaternion rotation)
    {
        PooledProjectile p = null;

        while (_pool.Count > 0 && p == null)
            p = _pool.Dequeue(); // skip destroyed entries just in case

        if (p == null)
        {
            if (!allowGrowth || projectilePrefab == null)
                return null;

            p = CreateInstance();
        }

        p.transform.SetPositionAndRotation(position, rotation);
        p.gameObject.SetActive(true);
        return p;
    }

    public void Return(PooledProjectile p)
    {
        if (p == null) return;

        p.gameObject.SetActive(false);
        p.transform.SetParent(transform, true);
        _pool.Enqueue(p);
    }
}