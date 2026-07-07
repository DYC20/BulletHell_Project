using System;
using UnityEngine;
using UnityEngine.VFX;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.Rendering;
using VisualEffect = UnityEngine.VFX.VisualEffect;

[RequireComponent(typeof(Collider2D))]
public class PooledProjectile : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private ProjectileMovement projMovement;
    [SerializeField] private ProjectileHitEffect hitEffect;
    [SerializeField] private ProjectileHitEffectPS hitEffectPS;
    [SerializeField] private ProjectileShootEffect shootEffect;
    [SerializeField] private ProjectileShootEffectPS shootEffectPS;
    [SerializeField] private Renderer bulletRenderer;
    [SerializeField, ColorUsage(false, true)] private  Color enemyBulletColor;
    [SerializeField] private ParticleSystem trailPS;
    [SerializeField] private GameObject notTrailObjects;
    
    //[SerializeField]  private Renderer OutlineMaterialRenderer;
    //private MaterialPropertyBlock mpb;

    // Runtime state
    private GameObject _owner;
    private Teams _ownerTeam;
    
    private ProjectileConfigSO _config;
    private ProjectileStats _stats;
    private List<ProjectileModifierSO> _mods;
    private Quaternion projectileOrientation;
    private ProjectileModifierSet _modifierSet;
    private AmmoType _shotAmmoType; 
    [ColorUsage(false, true)] private Color originalColor;
    [ColorUsage(false, true)] private Color bulletNewColor;
    private MaterialPropertyBlock mpb;
    
    private bool isGrounded;
    private bool targetIsGrounded;
    private bool isSniper;
    
    private int _remainingPierce;
    private float _lifeTimer;

    private Transform target;

    private IDamageable damageable;
    private ModifierSO activeModifier;
    
    private int _shotID;

    private int _particleCount;
    private bool trailIsAlive;
    private bool forceDespawn;
    
/*
    private void Awake()
    {
        
        if (OutlineMaterialRenderer == null) OutlineMaterialRenderer = GetComponent<Renderer>();
        mpb ??= new MaterialPropertyBlock();
    }
*/
    private void Reset()
    {
       // OutlineMaterialRenderer = GetComponent<Renderer>();
        rb = GetComponent<Rigidbody2D>();
        projMovement = GetComponent<ProjectileMovement>();
        hitEffect = GetComponent<ProjectileHitEffect>();
        hitEffectPS = GetComponent<ProjectileHitEffectPS>();
        shootEffect = GetComponent<ProjectileShootEffect>();
        shootEffectPS = GetComponent<ProjectileShootEffectPS>();
        notTrailObjects.SetActive(true);
        forceDespawn = false;
    }

    private void Awake()
    {
        if (mpb == null)
            mpb = new MaterialPropertyBlock();

        if (bulletRenderer == null)
            bulletRenderer = GetComponentInChildren<Renderer>();
    }

    private void OnEnable()
    {
        // Important: ensure collider works immediately, but timer resets on Init
        _lifeTimer = 0f;
    }

    private void ChangeBulletColor(Color bulletNewColor)
    {
        if (bulletRenderer == null)
            return;
        
        bulletRenderer.GetPropertyBlock(mpb);
        mpb.SetColor("_Color", bulletNewColor);
        bulletRenderer.SetPropertyBlock(mpb);
    }

    public void Init(GameObject owner, Teams ownerTeam, ProjectileConfigSO config, Vector2 direction, float speedOverride, Transform spawnTf, int shotid)
    {
        forceDespawn = false;
        trailIsAlive = trailPS != null;

        if (notTrailObjects != null)
            notTrailObjects.SetActive(true);

        if (trailPS != null)
        {
            trailPS.Clear(true);
            trailPS.Play(true);
        }
        
        Debug.LogWarning("SpeedOverride: " + speedOverride);
        _shotID = shotid;
        _owner = owner;
        _shotAmmoType = config != null ? config.ammoType : AmmoType.Bullet;
        _modifierSet = _owner != null ? _owner.GetComponentInParent<ProjectileModifierSet>() : null;
        _ownerTeam = ownerTeam;
        _config = config;

        
        originalColor = bulletRenderer.sharedMaterial.color;

        if (_ownerTeam == Teams.Enemy)
        {
            bulletNewColor = enemyBulletColor;
            ChangeBulletColor(bulletNewColor);
        }
        
        if (_ownerTeam == Teams.Player)
        {
            bulletNewColor = originalColor;
            ChangeBulletColor(bulletNewColor);
        }
        
        if (!TryGetGroundedState(owner, out isGrounded))
        {
            Debug.LogWarning($"Could not determine grounded state for owner {owner.name}");
            isGrounded = true; // optional fallback
        }
        Debug.Log($"owner: {owner} ,is grounded:+ {isGrounded}");
        
        /*
        Debug.Log($"[INIT] proj={name} cfg={_config.name} ammo={_config.ammoType} " +
                  $"shootComp(VFX)={(shootEffect!=null)} shootList(VFX)={_config.shootEffect?.Count ?? -1} " +
                  $"shootComp(PS)={(shootEffectPS!=null)} shootList(PS)={_config.shootEffectPS?.Count ?? -1}");
        */
        _remainingPierce = config != null ? config.pierceCount : 0;
        //Set Modifiers
        _stats = ProjectileStatsBuilder.FromConfig(config);
        //_mods = modifiers != null ? new List<ProjectileModifierSO>(modifiers) : null;
        //Color _baseColor = OutlineMaterialRenderer.material.color;
        /*
        if (_mods != null)
            for (int i = 0; i < _mods.Count; i++)
                _mods[i].ModifyStats(ref _stats);
        
        if (mpb != null && _mods != null)
        {
            OutlineMaterialRenderer.GetPropertyBlock(mpb);
            mpb.Clear();
            
            mpb.SetColor("_Color", _baseColor);
            for (int i = 0; i < _mods.Count; i++)
                _mods[i].ApplyVisuals(mpb);
            //Debug.Log($"Applying MPB color: {mpb.GetVector("_Color")} to {name}");
            OutlineMaterialRenderer.SetPropertyBlock(mpb);
        }
*/
        _lifeTimer = 0f;

        float speed = speedOverride > 0f ? speedOverride : config.speed;
        Debug.LogWarning("Speed override check: " + speed);

        if (projMovement != null)
        {
            projMovement.Apply(rb, config, direction, speed);
            Debug.LogWarning("proj move check, Projectile Movement apply: " + speed);
        }
            
        
        else if (rb != null && config != null)
        {
            rb.linearVelocity = direction.normalized * speed;
            Debug.LogWarning("rb move check, Projectile Linear Velocity: " + rb.linearVelocity);
        }
            
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg-90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        projectileOrientation = transform.rotation;

        if (shootEffect != null && _config != null && spawnTf != null)
        {
            for (int FX = 0; FX < _config.shootEffect.Count; FX++)
            {
                VisualEffect shootFX = _config.shootEffect[FX];
                shootEffect.Apply(shootFX, spawnTf.position, projectileOrientation);
                //Debug.Log("ShootEffect Applied:"+ shootFX.name);
            }
           
        }
        
        else if (shootEffect == null || _config == null || spawnTf == null)
        {
            Debug.LogWarning("shoot effect/config/spawnTF is null");
        }
        
        if (shootEffectPS != null && _config != null && spawnTf != null)
        {
            for (int FX = 0; FX < _config.shootEffectPS.Count; FX++)
            {
                ParticleSystem shootFX = _config.shootEffectPS[FX];
                //Debug.Log($"[MUZZLE PS] cfg={_config.name} index={FX} ps={(shootFX ? shootFX.name : "NULL")}");
                if (shootFX == null) continue;
                shootEffectPS.Apply(shootFX, spawnTf.position, projectileOrientation);
                //Debug.Log("ShootEffect PS Applied:" + shootFX.name);
            }
            
        }
        if (trailPS != null)
            trailIsAlive = true;
        if (trailPS == null)
            _particleCount = 0;
        /*
        else if (shootEffect == null || _config == null || spawnTf == null)
        {
            Debug.LogWarning("shoot effect/config/spawnTF is null");
        }
        */
    }

    private void Update()
    {
        if (forceDespawn)
        {
            if (trailPS == null || !trailPS.IsAlive(true))
            {
                FinishDespawn();
            }

            return;
        }

        if (_config == null) return;

        _lifeTimer += Time.deltaTime;

        if (_config.doDissapateOverLifetime)
        {
            float t = (_config.lifetime <= 0f) ? 1f : Mathf.Clamp01(_lifeTimer / _config.lifetime);
            float scale = _config.dissapateOverLifetime.Evaluate(t);
            transform.localScale = Vector3.one * scale;
        }

        if (_lifeTimer >= _config.lifetime)
        {
            Despawn();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
       // Debug.Log($"Projectile triggered with: {other.name}, layer: {LayerMask.LayerToName(other.gameObject.layer)}, root: {other.transform.root.name}");
        if (other.gameObject.layer == LayerMask.NameToLayer("Walls"))
        {
            if (isGrounded)
            {
                projectileOrientation = Quaternion.Euler(projectileOrientation.eulerAngles.x,
                    projectileOrientation.eulerAngles.y
                    , projectileOrientation.eulerAngles.z - 90f
                );
                
                /*if (_config.AOE > 0f)
                {
                    CreateShockwave();
                }*/
                
                if (hitEffect != null && _config != null)
                {
                    Debug.Log("projectileOrientation:" + projectileOrientation);
                    for (int FX = 0; FX < _config.wallhitEffect.Count; FX++)
                    {

                        VisualEffect hitFX = _config.wallhitEffect[FX];
                        hitEffect.Apply(hitFX, transform.position, projectileOrientation);
                        Debug.Log("hitEffect Applied:" + hitFX.name);
                    }

                }

                
                else if (hitEffect == null || _config == null)
                {
                    Debug.LogWarning("hit effect/config is null");
                }
            
                if (hitEffectPS != null && _config != null)
                {
                    for (int FX = 0; FX < _config.wallhitEffectPS.Count; FX++)
                    {
                        ParticleSystem hitFX = _config.wallhitEffectPS[FX];
                        Debug.Log($"[WALL HIT PS] cfg={_config.name} index={FX} ps={(hitFX ? hitFX.name : "NULL")}");
                        if (hitFX == null) continue;
                        hitEffectPS.Apply(hitFX, transform.position, projectileOrientation);
                        Debug.Log("hit PS Applied"+ hitFX.name);
                    }

                }

                
                else if (hitEffect == null || _config == null)
                {
                    Debug.LogWarning("hit effect/config is null");
                }
                
                Despawn();
            }

            return; //hit a wall
        }

        SimplePistol_Waepon weaponCOllider = other.GetComponent<SimplePistol_Waepon>();
        if (weaponCOllider)
        {
            return;
        }
        
        if (_config == null) return;

        // Optional layer mask filter
        if (((1 << other.gameObject.layer) & _config.hitMask.value) == 0)
            return;

        // Don’t hit owner
        if (_owner != null && other.gameObject == _owner)
            return;
        
        // Friendly fire rule (team check requires target has Health/IDamageable with a team)
        IDamageable hitDamageable = other.GetComponentInParent<IDamageable>();
        
        if (hitDamageable == null)
        {
            Debug.Log("hitDamageable is null on " + other.name);
            return;
        }
        //Debug.Log($"hitDamageable: {other.name}");

        if (_config.preventFriendlyFire && hitDamageable.Team == _ownerTeam)
            return;

        GameObject hitTarget = ((MonoBehaviour)hitDamageable).gameObject;
        Debug.LogWarning("hit target: " + hitTarget);
        Transform VisualCenter = hitTarget.transform.Find("VisualCenter(DNCN)");
        Debug.LogWarning("VisualCenter: " + VisualCenter);
        
        target = VisualCenter;
        damageable = hitDamageable;
        
        if (!TryGetGroundedState(hitTarget, out bool targetIsGrounded))
        {
            Debug.LogWarning($"Could not determine grounded state for hit target {hitTarget.name}");
            return;
        }

        if (isGrounded != targetIsGrounded)
        {
            if(isSniper ) 
                HitTarget();
            Debug.Log($"Ground mismatch. Shooter: {isGrounded}, Target: {targetIsGrounded}");
            return;
        }
        HitTarget();
    }
        //Debug.Log($"[CFG CHECK] cfgName={_config.name} cfgID={_config.GetInstanceID()} path={UnityEditor.AssetDatabase.GetAssetPath(_config)}");
/*
        for (int i = 0; i < _config.hitEffectPS.Count; i++)
        {
            var ps = _config.hitEffectPS[i];
            //Debug.Log($"[CFG hitEffectPS] i={i} val={(ps ? ps.name : "NULL")}");
        }

        for (int i = 0; i < _config.hitEffect.Count; i++)
        {
            var vfx = _config.hitEffect[i];
            //Debug.Log($"[CFG hitEffect VFX] i={i} val={(vfx ? vfx.name : "NULL")}");
        }
*/
    /*
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            target = other.gameObject;
            targetIsGrounded = target.GetComponentInChildren<SpriteRenderer>().sortingLayerName == ("Ground");

            if (targetIsGrounded && isGrounded)
            {
                HitTarget();
            }
            if (targetIsGrounded == false && isGrounded == false)
            {
                HitTarget();
            }
        }

    }
*/
    private void HitTarget()
{
    activeModifier = _modifierSet != null
        ? _modifierSet.GetModifierFor(_shotAmmoType) as ModifierSO
        : null;

    // NON-AOE: keep old counted behavior
    if (_config.AOE <= 0f)
    {
        _modifierSet?.NotifyHitEnemy(
            _shotAmmoType,
            _owner,
            target.gameObject,
            target.transform.position,
            target.transform.rotation,
            _shotID
        );
    }

    // direct-hit FX
    if (hitEffect != null && _config != null)
    {
        for (int FX = 0; FX < _config.hitEffect.Count; FX++)
        {
            VisualEffect hitFX = _config.hitEffect[FX];
            hitEffect.Apply(hitFX, target.transform.position, target.transform.rotation);
        }
    }

    if (hitEffectPS != null && _config != null)
    {
        for (int FX = 0; FX < _config.hitEffectPS.Count; FX++)
        {
            ParticleSystem hitFX = _config.hitEffectPS[FX];
            if (hitFX == null) continue;

            hitEffectPS.Apply(hitFX, target.transform.position, target.transform.rotation);
        }
    }

    // AOE: debuff everyone in radius immediately, with NO counting
    if (_config.AOE > 0f)
    {
        CreateShockwave();
    }

    // direct-hit damage
    damageable.TakeDamage(_config.damage, _owner);
    //Debug.Log("Take Damage Owner Variable:" + _owner.name);

    if (_config.destroyOnHit)
    {
        Despawn();
        return;
    }

    if (_remainingPierce > 0)
    {
        _remainingPierce--;
        if (_remainingPierce <= 0)
            Despawn();
    }
    else
    {
            Despawn();
    }
}

    private void CreateShockwave()
    {
        float radius = _config.AOE;
        Vector3 aoeCenter = transform.position;

        DrawCircle(aoeCenter, radius, 32, 1f);

        Collider2D[] aoeHits = Physics2D.OverlapCircleAll(aoeCenter, radius);
        HashSet<IDamageable> processed = new HashSet<IDamageable>();

        //PlayerWeaponController weaponController = _owner.GetComponentInParent<PlayerWeaponController>();
        //weaponController.shockWave.PlayShockwave(aoeCenter);
        GameObject shockwaveObject = Instantiate(_config.shockWavePrefab, aoeCenter, Quaternion.identity);
        
        ShockWave shockwaveEffect = shockwaveObject.GetComponent<ShockWave>();
        
        if (shockwaveEffect != null)
        {
            shockwaveEffect.PlayShockwave();
            Debug.Log($"Shockwave effect: {shockwaveEffect.name}");
        }
        else
        {
            Debug.LogWarning("ShockWave component missing on spawned prefab.");
        }


        foreach (var hit in aoeHits)
        {
            IDamageable aoeDamageable = hit.GetComponentInParent<IDamageable>();
            if (aoeDamageable == null) continue;
            if (aoeDamageable.Team == _ownerTeam) continue;
            if (!processed.Add(aoeDamageable)) continue;

            MonoBehaviour mb = aoeDamageable as MonoBehaviour;
            if (mb == null) continue;

            GameObject enemyGO = mb.gameObject;
            Vector3 fxPos = hit.bounds.center;

            if (enemyGO == target.gameObject)
            {
                activeModifier?.ApplyDebuffDirect(_owner, enemyGO, enemyGO.transform.rotation);
                continue;
            }

            // AOE splash damage
            aoeDamageable.TakeDamage(_config.damage / 3f, _owner);

            // immediate debuff, no counting
            activeModifier?.ApplyDebuffDirect(_owner, enemyGO, enemyGO.transform.rotation);

            // AOE FX
            if (hitEffect != null && _config != null)
            {
                for (int FX = 0; FX < _config.hitEffect.Count; FX++)
                {
                    VisualEffect hitFX = _config.hitEffect[FX];
                    hitEffect.Apply(hitFX, fxPos, enemyGO.transform.rotation);
                }
            }

            if (hitEffectPS != null && _config != null)
            {
                for (int FX = 0; FX < _config.hitEffectPS.Count; FX++)
                {
                    ParticleSystem hitFX = _config.hitEffectPS[FX];
                    if (hitFX == null) continue;

                    hitEffectPS.Apply(hitFX, fxPos, enemyGO.transform.rotation);
                }
            }
        }
    }

    public void Despawn()
    {
        if (forceDespawn)
            return;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (notTrailObjects != null)
            notTrailObjects.SetActive(false);

        if (trailPS != null)
        {
            forceDespawn = true;

            // Stop creating new particles, but let existing particles finish.
            trailPS.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            Debug.Log("Started forced despawn, waiting for trail particles.");
            return;
        }

        FinishDespawn();
    }
    private void FinishDespawn()
    {
        forceDespawn = false;
        trailIsAlive = false;

        _owner = null;
        _config = null;

        gameObject.SetActive(false);

        Debug.Log("Projectile fully despawned after trail ended.");
    }
    
    private bool TryGetGroundedState(GameObject obj, out bool grounded)
    {
        grounded = false;

        PlayerController playerController = obj.GetComponentInParent<PlayerController>();
        if (playerController != null)
        {
            grounded = playerController.isPlayerGrounded;
            return true;
        }

        EnemyChaseAI enemyAI = obj.GetComponentInParent<EnemyChaseAI>();
        if (enemyAI != null)
        {
            grounded = enemyAI.isEnemyGrounded;
            return true;
        }

        EnemyShooterWalkingAI enemyShooterWalkingAI = obj.GetComponentInParent<EnemyShooterWalkingAI>();
        if (enemyShooterWalkingAI != null)
        {
            grounded = enemyShooterWalkingAI.isEnemyGrounded;
            return true;
        }
        
        EnemyShooterAI enemyShooterAI = obj.GetComponentInParent<EnemyShooterAI>();
        if (enemyShooterAI != null)
        {
            grounded = enemyShooterAI.isEnemyGrounded;
            isSniper = true;
            return true;
        }
        
        
   

        Debug.LogWarning($"No grounded-state component found on {obj.name}");
        return false;
    }
    void DrawCircle(Vector3 center, float radius, int segments = 32, float duration = 1f)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0f, 0f);

        for (int i = 1; i <= segments; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);

            Debug.DrawLine(prevPoint, newPoint, Color.red, duration);
            prevPoint = newPoint;
        }
    }
}
