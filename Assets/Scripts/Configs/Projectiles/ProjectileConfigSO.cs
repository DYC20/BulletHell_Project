using UnityEngine;
using UnityEngine.VFX;

[CreateAssetMenu(menuName = "Projectile Config", fileName = "ProjectileConfig")]
public class ProjectileConfigSO : ScriptableObject
{
    [Header("Ammo")]
    public AmmoType ammoType = AmmoType.Bullet;

    [Min(0)]
    public int ammoPerShot = 1;

    [Header("Damage")]
    public float damage = 1f;

    [Header("Movement")]
    public float speed = 18f;
    public float lifetime = 1.2f;
    
    [Header("Spread / Multi-shot")]
    public int projectilesPerShot = 1;     // 1 = normal, 6-12 = shotgun
    public float spreadAngleDeg = 0f;      // total cone angle (e.g. 10 = small, 45 = shotgun)
    public bool randomSpread = true;       // random within cone
    public bool centerProjectile = true;   // for odd counts, keep one in the middle
    public float spawnPosJitter = 0f;      // small random offset for muzzle scatter
    public float speedMultiplierMin = 1f;  // e.g. 0.85
    public float speedMultiplierMax = 1f;  // e.g. 1.15

    [Header("Hit Rules")]
    public bool destroyOnHit = true;
    
    [Header("Hit Effect")]
    public VisualEffect impactEffect;
    
    [Header("Shoot Effect")]
    public VisualEffect shootEffect;

    [Header("Dissapate")] 
    public bool dissapateOverLifetime;

    [Tooltip("If enabled, projectile will ignore targets on same team as owner.")]
    public bool preventFriendlyFire = true;

    [Tooltip("Optional: only hit these layers. Leave empty for 'hit anything with IDamageable'.")]
    public LayerMask hitMask = ~0;

    [Header("Pierce (optional)")]
    [Tooltip("0 = no piercing. 1 = can hit 1 target then despawn, etc.")]
    public int pierceCount = 0;
    

}

