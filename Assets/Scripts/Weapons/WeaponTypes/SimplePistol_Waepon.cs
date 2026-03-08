using UnityEngine;
using Unity.Cinemachine;
using Unity.VectorGraphics;

public class SimplePistol_Waepon : WeaponBase, IWeaponProjectileBase
{
    [Header("Projectile")]
    //[SerializeField] private ProjectileId projectileId = ProjectileId.SimplePistol_Bullet;         // Pool_BulletStandard
    [SerializeField] private ProjectileConfigSO projectileConfig; // PistolProjectileConfig

    [SerializeField] private ObjectPool ProjectilePool;
    
    private SpriteRenderer rd;
    

    private int projectileLowLayer;
    private int projectileHighLayer;

    [Header("Camera Shake")]
    [SerializeField] private CinemachineImpulseSource recoilImpulse;
    [SerializeField] private float recoilStrength = 0.2f;

    private bool _IsSpriteRenderer = false;
    
    //private Vector3 firePointDefaultLocalPos; //firepoint orientation
    //private bool lastFlipState;

    protected void Awake()
    {
        _IsSpriteRenderer = TryGetComponent<SpriteRenderer>(out rd);
        projectileLowLayer = LayerMask.NameToLayer("Projectile_Low");
        projectileHighLayer = LayerMask.NameToLayer("Projectile_High");
            
    }

    protected override bool CanFire()
    {
        var set = owner != null ? owner.GetComponentInParent<ProjectileModifierSet>() : null;
        
        ProjectileConfigSO cfg = projectileConfig;
        ObjectPool pool = ProjectilePool;

        if (set != null)
            set.ApplyForCurrentAmmo(projectileConfig.ammoType, ref cfg, ref pool);
        
        if (cfg == null)
        {
            Debug.LogWarning($"{name}: projectileConfig is null.");
            return false;
        }

        if (cfg.ammoPerShot <= 0) return true;
        if (ammoConsumer == null) return false;

        return ammoConsumer.HasAmmo(cfg.ammoType, cfg.ammoPerShot);
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
    var set = owner != null ? owner.GetComponentInParent<ProjectileModifierSet>() : null;
    
    AmmoType currentAmmo = projectileConfig.ammoType;
  
    ProjectileConfigSO cfg = projectileConfig;
    ObjectPool pool = ProjectilePool;
    
    if (set != null)
        set.ApplyForCurrentAmmo(currentAmmo, ref cfg, ref pool);
    
    if (cfg == null || pool == null) return;

    // consume ammo based on cfg
    if (cfg.ammoPerShot > 0)
    {
        var ammo = owner != null ? owner.GetComponentInParent<IAmmoConsumer>() : null;
        if (ammo == null) return;

        if (!ammo.TryConsumeAmmo(cfg.ammoType, cfg.ammoPerShot))
            return;
    }

    int count = Mathf.Max(1, cfg.projectilesPerShot);
    float cone = Mathf.Max(0f, cfg.spreadAngleDeg);
    
    Vector2 baseDir = firePoint.up;


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
            float t = (count == 1) ? 0.5f : (i / (float)(count - 1));
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
            rd.flipY = this.transform.rotation.eulerAngles.z < 270 && this.transform.rotation.eulerAngles.z > 90;
         
            //bool oriantation = this.transform.parent.rotation.eulerAngles.z < 45f && this.transform.parent.rotation.eulerAngles.z > -45;
            if(this.transform.parent != null)rd.sortingOrder = Vector3.Dot(this.transform.parent.up, Vector3.up) < 0.6f ? 1 : -1;
        }
    }

   
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

    public ProjectileConfigSO BaseConfig => projectileConfig;
}