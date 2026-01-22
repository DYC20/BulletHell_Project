using UnityEngine;
using Unity.Cinemachine;

public class SimplePistol_Waepon : WeaponBase
{
    [Header("Projectile")]
    [SerializeField] private ProjectileId projectileId = ProjectileId.SimplePistol_Bullet;         // Pool_BulletStandard
    [SerializeField] private ProjectileConfigSO projectileConfig; // PistolProjectileConfig
    
    [Header("Camera Shake")]
    [SerializeField] private CinemachineImpulseSource recoilImpulse;
    [SerializeField] private float recoilStrength = 0.2f;
    
    protected override bool CanFire()
    {
        if (projectileConfig == null)
        {
            Debug.LogWarning($"{projectileConfig}: is null.");
            return false;
        }

        // If ammoPerShot is 0, treat as infinite ammo for this weapon.
        if (projectileConfig.ammoPerShot <= 0) return true;

        if (ammoConsumer == null) return false;

        return ammoConsumer.HasAmmo(projectileConfig.ammoType, projectileConfig.ammoPerShot);
        /*       // Owner must have ammo inventory to consume ammo
        var ammo = owner != null ? owner.GetComponentInParent<IAmmoConsumer>() : null;
        if (ammo == null)
        {
            Debug.LogWarning("Ammo is null.");
            return false;
        }

        return ammo.HasAmmo(projectileConfig.ammoType, projectileConfig.ammoPerShot);*/
    }
    protected override void FireInternal()
    {
        if (projectileConfig == null)
        {
            Debug.LogWarning($"{name}: Missing pool or config.");
            return;
        }

        var pool = PoolRegistry.Instance != null
            ? PoolRegistry.Instance.GetPool(projectileId)
            : null;

        if (pool == null)
            return;
        
        if (projectileConfig.ammoPerShot > 0)
        {
            var ammo = owner != null ? owner.GetComponentInParent<IAmmoConsumer>() : null;
            if (ammo == null) return;

            if (!ammo.TryConsumeAmmo(projectileConfig.ammoType, projectileConfig.ammoPerShot))
            {
                Debug.Log($"{name}: Projectile {projectileConfig.ammoType} TryConsumeAmmo Failed");
                return; // no ammo => no shot
            }
            Debug.Log($"{name}: Projectile {projectileConfig.ammoType} TryConsumeAmmo Successful");
        }
        
        var proj = pool.Get(firePoint.position, firePoint.rotation);
        if (proj == null)
            return;

        // In your setup, firePoint.up should be your forward shooting direction.
        Vector2 fireDirection = firePoint.up;
        var mods = owner != null ? owner.GetComponentInParent<ProjectileModifierSet>() : null;
        proj.Init(owner, ownerTeam, projectileConfig, fireDirection, firePoint, mods != null ? mods.Active : null);
        

        if (recoilImpulse != null)
        {
            Vector2 recoilDir = fireDirection;
            recoilImpulse.GenerateImpulse(new Vector3(recoilDir.x, recoilDir.y, 0f) * recoilStrength);
        }
    }
}