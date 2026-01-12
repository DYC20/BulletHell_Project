/*
using UnityEngine;
using UnityEngine.VFX;

public class BulletDamage : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    public GameObject owner;

    private PooledBullet pooled;

    private void Awake()
    {
        pooled = GetComponent<PooledBullet>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (owner != null && other.gameObject == owner)
            return;

        var health = other.GetComponent<Health>();
        if (health != null)
            //health.TakeDamage(damage);

        // Return to pool instead of Destroy
        if (pooled != null) pooled.Release();
        else gameObject.SetActive(false); // fallback
    }
}
*/
