using UnityEngine;
using Unity.Cinemachine;
using Unity.VectorGraphics;

public class SimplePistol_Waepon : WeaponBase
{
    [Header("Projectile")]
    [SerializeField] private ProjectileId projectileId = ProjectileId.SimplePistol_Bullet;         // Pool_BulletStandard
    [SerializeField] private ProjectileConfigSO projectileConfig; // PistolProjectileConfig

    [SerializeField] private SpriteRenderer rd;
    [SerializeField] private ObjectPool ProjectilePool;
    
    private int projectileLowLayer;
    private int projectileHighLayer;
    
    [Header("Camera Shake")]
    [SerializeField] private CinemachineImpulseSource recoilImpulse;
    [SerializeField] private float recoilStrength = 0.2f;
    
    protected void Awake()
    {
        rd = GetComponent<SpriteRenderer>();
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
        if (projectileConfig == null) return;

        //var pool = PoolRegistry.Instance != null ? PoolRegistry.Instance.GetPool(projectileId) : null;
        //if (pool == null) return;

        // Consume ammo ONCE per shot (not per pellet) — typical shotgun behavior.
        // If you want ammo per pellet, multiply by projectilesPerShot.
        if (projectileConfig.ammoPerShot > 0)
        {
            var ammo = owner != null ? owner.GetComponentInParent<IAmmoConsumer>() : null;
            if (ammo == null) return;

            if (!ammo.TryConsumeAmmo(projectileConfig.ammoType, projectileConfig.ammoPerShot))
                return;
        }

        int count = Mathf.Max(1, projectileConfig.projectilesPerShot);
        float cone = Mathf.Max(0f, projectileConfig.spreadAngleDeg);

        Vector2 baseDir = firePoint.up;

        // Pre-calc modifiers once
        var mods = owner != null ? owner.GetComponentInParent<ProjectileModifierSet>() : null;
        var activeMods = mods != null ? mods.Active : null;

        // Determine layer once (based on root layer)
        int playerHighLayer = LayerMask.NameToLayer("Player_High");
        var shooterRoot = owner != null ? owner.transform.root.gameObject : null;
        bool fromHighland = shooterRoot != null && shooterRoot.layer == playerHighLayer;
        int projectileLayer = fromHighland ? projectileHighLayer : projectileLowLayer;

        Debug.LogWarning($"SHOTGUN DEBUG: count={count}, spread={cone}, frame={Time.frameCount}");
        for (int i = 0; i < count; i++)
        {
            GameObject proj = ProjectilePool.GetInstance(firePoint.position,  Quaternion.identity);
            //var proj = pool.Get(firePoint.position,  Quaternion.identity);
            Debug.LogWarning(
                $"pellet {i}/{count - 1}: proj={(proj ? proj.name : "NULL")} " +
                $"id={(proj ? proj.GetInstanceID().ToString() : "null")} " +
                $"active={(proj ? proj.gameObject.activeSelf.ToString() : "n/a")}");
            if (proj == null) continue;

            SetLayerRecursively(proj.gameObject, projectileLayer);

            // Spread: either random within cone, or evenly spaced across cone
            float angle;
            if (cone <= 0f || count == 1)
            {
                angle = 0f;
            }
            else if (projectileConfig.randomSpread)
            {
                angle = Random.Range(-cone * 0.5f, cone * 0.5f);
            }
            else
            {
                // Even spacing from -cone/2 to +cone/2
                float t = (count == 1) ? 0.5f : (i / (float)(count - 1));
                angle = Mathf.Lerp(-cone * 0.5f, cone * 0.5f, t);
            }

            Vector2 dir = Rotate(baseDir, angle);
            float mult = Random.Range(projectileConfig.speedMultiplierMin, projectileConfig.speedMultiplierMax);
            float pelletSpeed = projectileConfig.speed * mult;
            var pg = proj.GetComponent<PooledProjectile>();
            pg.Init(owner, ownerTeam, projectileConfig, dir, pelletSpeed, firePoint, activeMods);
        }

        // Recoil once per shot
        if (recoilImpulse != null)
        {
            recoilImpulse.GenerateImpulse(new Vector3(baseDir.x, baseDir.y, 0f) * recoilStrength);
        }
    }

    private void Update()
    {
        rd.flipY = this.transform.rotation.eulerAngles.z < 270 && this.transform.rotation.eulerAngles.z  > 90;
    }

    /*   Old FireInternal
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
    //Debug.Log($"{name}.SetOwner called for {owner.name}");

    /*GameObject shooterRoot = owner != null && owner.GetComponent<Rigidbody2D>() != null
        ? owner
        : owner != null ? owner.transform.root.gameObject : null;*/
    /* var shooterRoot = owner != null ? owner.transform.root.gameObject : null;


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
 }*/
    //Helpers//
    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
    private static Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(rad);
        float cos = Mathf.Cos(rad);
        return new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y);
    }

}