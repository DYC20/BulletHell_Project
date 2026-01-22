using UnityEngine;
using UnityEngine.VFX;

public struct ProjectileStats
{
    [Header("Ammo")]
    public AmmoType ammoType;

    [Min(0)]
    public int ammoPerShot ;

    [Header("Damage")]
    public float damage ;

    [Header("Movement")]
    public float speed ;
    public float lifetime ;

    [Header("Hit Rules")]
    public bool destroyOnHit ;
    
    [Header("Hit Effect")]
    public VisualEffect impactEffect;
    
    [Header("Shoot Effect")]
    public VisualEffect shootEffect;

    [Tooltip("If enabled, projectile will ignore targets on same team as owner.")]
    public bool preventFriendlyFire;

    [Tooltip("Optional: only hit these layers. Leave empty for 'hit anything with IDamageable'.")]
    public LayerMask hitMask ;

    [Header("Pierce (optional)")]
    [Tooltip("0 = no piercing. 1 = can hit 1 target then despawn, etc.")]
    public int pierceCount ;
}