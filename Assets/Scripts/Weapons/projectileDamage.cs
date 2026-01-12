/*
using UnityEngine;

public class projectileDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float damage = 10f;
    
    [HideInInspector] public GameObject owner;
    [SerializeField] private GameObject impactEffect;

    private PooledBullet pooled;
    
    private void Awake()
    {
        pooled = GetComponent<PooledBullet>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (owner == null)
        {
          //  Debug.LogError($"{name}: Bullet owner is NULL. Weapon did not assign it.");
        }
        
        // Don't hit the thing that fired the bullet
        if (owner != null && other.gameObject == owner)
            return;

        var health = other.GetComponent<Health>();
        if (health != null)
            //health.TakeDamage(damage);
        
        if(impactEffect)
            Instantiate(impactEffect, transform.position, Quaternion.identity);

        // Return to pool (NOT Destroy)
        if (pooled != null) pooled.Release();
        else gameObject.SetActive(false);
    }
}
*/
