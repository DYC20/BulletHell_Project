using UnityEngine;
using Unity.Cinemachine;
using Unity.VectorGraphics;

public class SimplePistol_Waepon : WeaponBase, IWeaponProjectileBase
{
    [Header("Projectile")]
    [SerializeField] private ProjectileConfigSO projectileConfig;
    [SerializeField] private ObjectPool projectilePool;

    private ProjectileConfigSO defaultProjectileConfig;
    private ObjectPool defaultProjectilePool;
    private bool defaultsCached;

    [Header("Weapon")] 
    [SerializeField] private Sprite weaponUI;
    [SerializeField] private Transform weaponFX_tf;
    [SerializeField] private bool isRevolver;
    [SerializeField] private bool isShotgun;
    [SerializeField] private bool isGrenade;

    private SpriteRenderer rd;
    

    private int projectileLowLayer;
    private int projectileHighLayer;

    [Header("Camera Shake")]
    [SerializeField] private CinemachineImpulseSource recoilImpulse;
    [SerializeField] private float recoilStrength = 0.2f;

    private bool _IsSpriteRenderer = false;

    protected void Awake()
    {
        CacheDefaultsIfNeeded();

        _IsSpriteRenderer = TryGetComponent<SpriteRenderer>(out rd);
        projectileLowLayer = LayerMask.NameToLayer("Projectile_Low");
        projectileHighLayer = LayerMask.NameToLayer("Projectile_High");
    }
    
    private void CacheDefaultsIfNeeded()
    {
        if (defaultsCached) return;

        defaultProjectileConfig = projectileConfig;
        defaultProjectilePool = projectilePool;
        defaultsCached = true;
    }

    public override void ResetToDefaultProjectileData()
    {
        CacheDefaultsIfNeeded();

        projectileConfig = defaultProjectileConfig;
        projectilePool = defaultProjectilePool;
    }

    protected override bool CanFire()
    {
        CacheDefaultsIfNeeded();

        var set = owner != null ? owner.GetComponentInParent<ProjectileModifierSet>() : null;

        ProjectileConfigSO cfg = projectileConfig;
        ObjectPool pool = projectilePool;

        if (set != null && cfg != null)
            set.ApplyForCurrentAmmo(cfg.ammoType, ref cfg, ref pool);

        if (cfg == null)
        {
            Debug.LogWarning($"{name}: projectileConfig is null.");
            return false;
        }

        if (cfg.ammoPerShot <= 0) return true;
        if (ammoConsumer == null) return false;

        return ammoConsumer.HasAmmo(cfg.ammoType, cfg.ammoPerShot);
    }

    protected override void FireInternal()
    {
        CacheDefaultsIfNeeded();

        var set = owner != null ? owner.GetComponentInParent<ProjectileModifierSet>() : null;

        ProjectileConfigSO cfg = projectileConfig;
        ObjectPool pool = projectilePool;

        if (set != null && cfg != null)
            set.ApplyForCurrentAmmo(cfg.ammoType, ref cfg, ref pool);

        if (cfg == null || pool == null) return;

        if (cfg.ammoPerShot > 0)
        {
            var ammo = owner != null ? owner.GetComponentInParent<IAmmoConsumer>() : null;
            if (ammo == null) return;

            if (!ammo.TryConsumeAmmo(cfg.ammoType, cfg.ammoPerShot))
                return;
        }

        int count = Mathf.Max(1, cfg.projectilesPerShot);
        float cone = Mathf.Max(0f, cfg.spreadAngleDeg);

        Vector2 baseDir = transform.up;

        int playerHighLayer = LayerMask.NameToLayer("Player_High");
        var shooterRoot = owner != null ? owner.transform.root.gameObject : null;
        bool fromHighland = shooterRoot != null && shooterRoot.layer == playerHighLayer;
        int projectileLayer = fromHighland ? projectileHighLayer : projectileLowLayer;

        for (int i = 0; i < count; i++)
        {
            GameObject proj = pool.GetInstance(firePoint.position, Quaternion.identity);
            if (proj == null) continue;

            SetLayerRecursively(proj.gameObject, projectileLayer);

            float angle;
            if (cone <= 0f || count == 1) angle = 0f;
            else if (cfg.randomSpread) angle = Random.Range(-cone * 0.5f, cone * 0.5f);
            else
            {
                float t = count == 1 ? 0.5f : i / (float)(count - 1);
                angle = Mathf.Lerp(-cone * 0.5f, cone * 0.5f, t);
            }

            Vector2 dir = Rotate(baseDir, angle);
            float mult = Random.Range(cfg.speedMultiplierMin, cfg.speedMultiplierMax);
            float pelletSpeed = cfg.speed * mult;

            var pg = proj.GetComponent<PooledProjectile>();
            pg.Init(owner, ownerTeam, cfg, dir, pelletSpeed, firePoint);
        }

        if (recoilImpulse != null)
            recoilImpulse.GenerateImpulse(new Vector3(baseDir.x, baseDir.y, 0f) * recoilStrength);
    }

    private void Update()
    {
        if (_IsSpriteRenderer)
        {
            rd.flipY = transform.rotation.eulerAngles.z < 270 && transform.rotation.eulerAngles.z > 90;

            if (transform.parent != null)
                rd.sortingOrder = Vector3.Dot(transform.parent.up, Vector3.up) < 0.6f ? 1 : -1;
        }
    }

    public override AmmoType GetCurrentAmmoType()
    {
        CacheDefaultsIfNeeded();

        var set = owner != null ? owner.GetComponentInParent<ProjectileModifierSet>() : null;

        ProjectileConfigSO cfg = projectileConfig;
        ObjectPool pool = projectilePool;

        if (set != null && cfg != null)
            set.ApplyForCurrentAmmo(cfg.ammoType, ref cfg, ref pool);

        if (cfg == null)
            return defaultProjectileConfig != null ? defaultProjectileConfig.ammoType : default;

        return cfg.ammoType;
    }

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

    public ProjectileConfigSO BaseConfig => defaultProjectileConfig;
    public Transform WeaponFXtf => weaponFX_tf;

    public bool Revolver => isRevolver;
    public bool Shotgun => isShotgun;
    public bool Grenade => isGrenade;
    public Sprite UIImage => weaponUI;

    public Sprite WeaponImage => rd != null ? rd.sprite : null;
}