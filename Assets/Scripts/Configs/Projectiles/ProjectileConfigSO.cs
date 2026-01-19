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

    [Header("Hit Rules")]
    public bool destroyOnHit = true;
    
    [Header("Hit Effect")]
    public VisualEffect impactEffect;
    
    [Header("Shoot Effect")]
    public VisualEffect shootEffect;

    [Tooltip("If enabled, projectile will ignore targets on same team as owner.")]
    public bool preventFriendlyFire = true;

    [Tooltip("Optional: only hit these layers. Leave empty for 'hit anything with IDamageable'.")]
    public LayerMask hitMask = ~0;

    [Header("Pierce (optional)")]
    [Tooltip("0 = no piercing. 1 = can hit 1 target then despawn, etc.")]
    public int pierceCount = 0;
}

