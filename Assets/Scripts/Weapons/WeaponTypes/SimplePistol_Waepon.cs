using UnityEngine;
using Unity.Cinemachine;

public class SimplePistol_Waepon : WeaponBase
{
    [Header("Projectile")]
    [SerializeField] private ProjectileId projectileId = ProjectileId.SimplePistol_Bullet;         // Pool_BulletStandard
    [SerializeField] private ProjectileConfigSO projectileConfig; // PistolProjectileConfig
    
    private int projectileLowLayer;
    private int projectileHighLayer;
    
    [Header("Camera Shake")]
    [SerializeField] private CinemachineImpulseSource recoilImpulse;
    [SerializeField] private float recoilStrength = 0.2f;
    
    protected void Awake()
    {
        projectileLowLayer  = LayerMask.NameToLayer("Projectile_Low");
        projectileHighLayer = LayerMask.NameToLayer("Projectile_High");
    }
    
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

        int playerHighLayer = LayerMask.NameToLayer("Player_High");
        int playerLowLayer  = LayerMask.NameToLayer("Player_Low");
        Debug.Log($"{name}.SetOwner called for {owner.name}");
        
        /*GameObject shooterRoot = owner != null && owner.GetComponent<Rigidbody2D>() != null
            ? owner
            : owner != null ? owner.transform.root.gameObject : null;*/
        var shooterRoot = owner != null ? owner.transform.root.gameObject : null;
        
        
        Debug.Log($"owner={owner.name} layer={LayerMask.LayerToName(owner.layer)} ({owner.layer})");
        Debug.Log($"owner.root={owner.transform.root.name} layer={LayerMask.LayerToName(owner.transform.root.gameObject.layer)} ({owner.transform.root.gameObject.layer})");
        Debug.Log($"playerHighLayer={playerHighLayer}, playerLowLayer={playerLowLayer}");
        
        bool fromHighland = shooterRoot != null && shooterRoot.layer == playerHighLayer;

        int projectileLayer = fromHighland ? projectileHighLayer : projectileLowLayer;
        
        SetLayerRecursively(proj.gameObject, projectileLayer);
        Debug.LogWarning($"fromHighland={fromHighland} -> projectileLayer={LayerMask.LayerToName(projectileLayer)} ({projectileLayer})");
        Debug.LogWarning($"proj actual layer NOW = {LayerMask.LayerToName(proj.gameObject.layer)} ({proj.gameObject.layer})");

        Vector2 fireDirection = firePoint.up;
        var mods = owner != null ? owner.GetComponentInParent<ProjectileModifierSet>() : null;
        proj.Init(owner, ownerTeam, projectileConfig, fireDirection, firePoint, mods != null ? mods.Active : null);


        if (recoilImpulse != null)
        {
            Vector2 recoilDir = fireDirection;
            recoilImpulse.GenerateImpulse(new Vector3(recoilDir.x, recoilDir.y, 0f) * recoilStrength);
        }

        Debug.LogWarning(
            $"Projectile layer: {LayerMask.LayerToName(proj.gameObject.layer)} ({proj.gameObject.layer})"
        );
    }
    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}