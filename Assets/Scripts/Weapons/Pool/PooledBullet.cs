/*
using UnityEngine;

public class PooledBullet : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;
    
    private BulletPool pool;
    private float aliveUntil;

    public void SetPool(BulletPool pool)
    {
        this.pool = pool;
    }
    private void OnEnable()
    {
        aliveUntil = Time.time + lifetime;
    }

    private void Update()
    {
        if (Time.time >= aliveUntil)
            Release();
    }

    public void Release()
    {
        // stop physics
        var rb = GetComponent<Rigidbody2D>();
        if (rb) rb.linearVelocity = Vector2.zero;

        if (pool != null) pool.ReturnToPool(gameObject);
            else gameObject.SetActive(false); 
    }
}
*/
