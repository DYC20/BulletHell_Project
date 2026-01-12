/*
using UnityEngine;
using System.Collections.Generic;

public class BulletPool : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int prewarmCount = 50;

    private readonly Queue<GameObject> pool = new();
    
    public GameObject BulletPrefab => bulletPrefab;

    private void Awake()
    {
        Prewarm();
    }

    private void Prewarm()
    {
        for (int i = 0; i < prewarmCount; i++)
        {
            var b = CreateNew();
            ReturnToPool(b);
        }
    }

    private GameObject CreateNew()
    {
        var go = Instantiate(bulletPrefab, transform);
        var pooled = go.GetComponent<PooledBullet>();
        if (pooled == null)
            pooled = go.AddComponent<PooledBullet>();

        pooled.SetPool(this);
        return go;
    }

    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        var go = pool.Count > 0 ? pool.Dequeue() : CreateNew();
        
        // Skip destroyed references (safety)
        while (pool.Count > 0 && go == null)
            go = pool.Dequeue();

        if (go == null)
            go = CreateNew();

        go.transform.SetPositionAndRotation(position, rotation);
        
        go.SetActive(true);

        // Reset per-use state
        var rb = go.GetComponent<Rigidbody2D>();
        if (rb) rb.linearVelocity = Vector2.zero;

        return go;
    }

    public void ReturnToPool(GameObject bullet)
    {
        bullet.SetActive(false);
        bullet.transform.SetParent(transform);
        pool.Enqueue(bullet);
    }

}
*/
