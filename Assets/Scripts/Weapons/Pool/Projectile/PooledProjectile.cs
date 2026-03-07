using System;
using UnityEngine;
using UnityEngine.VFX;
using System.Collections.Generic;
using Unity.VisualScripting;

[RequireComponent(typeof(Collider2D))]
public class PooledProjectile : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private ProjectileMovement kinematics;
    [SerializeField] private ProjectileHitEffect hitEffect;
    [SerializeField] private ProjectileHitEffectPS hitEffectPS;
    [SerializeField] private ProjectileShootEffect shootEffect;
    [SerializeField] private ProjectileShootEffectPS shootEffectPS;
    
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
    

    private int _remainingPierce;
    private float _lifeTimer;
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
        kinematics = GetComponent<ProjectileMovement>();
        hitEffect = GetComponent<ProjectileHitEffect>();
        hitEffectPS = GetComponent<ProjectileHitEffectPS>();
        shootEffect = GetComponent<ProjectileShootEffect>();
        shootEffectPS = GetComponent<ProjectileShootEffectPS>();
    }

    private void OnEnable()
    {
        // Important: ensure collider works immediately, but timer resets on Init
        _lifeTimer = 0f;
    }

    public void Init(GameObject owner, Teams ownerTeam, ProjectileConfigSO config, Vector2 direction, float speedOverride, Transform spawnTf)
    {
        _owner = owner;
        _shotAmmoType = config != null ? config.ammoType : AmmoType.Bullet;
        _modifierSet = _owner != null ? _owner.GetComponentInParent<ProjectileModifierSet>() : null;
        _ownerTeam = ownerTeam;
        _config = config;
        
        Debug.Log($"[INIT] proj={name} cfg={_config.name} ammo={_config.ammoType} " +
                  $"shootComp(VFX)={(shootEffect!=null)} shootList(VFX)={_config.shootEffect?.Count ?? -1} " +
                  $"shootComp(PS)={(shootEffectPS!=null)} shootList(PS)={_config.shootEffectPS?.Count ?? -1}");
        
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
        
        if (kinematics != null)
            kinematics.Apply(rb, config, direction, speed);
        else if (rb != null && config != null)
            rb.linearVelocity = direction.normalized * config.speed;
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg-90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        projectileOrientation = transform.rotation;

        if (shootEffect != null && _config != null && spawnTf != null)
        {
            for (int FX = 0; FX < _config.shootEffect.Count; FX++)
            {
                VisualEffect shootFX = _config.shootEffect[FX];
                shootEffect.Apply(shootFX, spawnTf.position, spawnTf.rotation);
                Debug.Log("ShootEffect Applied:"+ shootFX.name);
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
                Debug.Log($"[MUZZLE PS] cfg={_config.name} index={FX} ps={(shootFX ? shootFX.name : "NULL")}");
                if (shootFX == null) continue;
                shootEffectPS.Apply(shootFX, spawnTf.position, spawnTf.rotation);
                Debug.Log("ShootEffect PS Applied:" + shootFX.name);
            }
            
        }
        else if (shootEffect == null || _config == null || spawnTf == null)
        {
            Debug.LogWarning("shoot effect/config/spawnTF is null");
        }
    }

    private void Update()
    {
        if (_config == null) return;
        
        _lifeTimer += Time.deltaTime;
         Debug.LogWarning("lifeTimer:" + _lifeTimer);
        if (_config.doDissapateOverLifetime == true)
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
        if (other.gameObject.layer == LayerMask.NameToLayer("Walls"))
        {
            projectileOrientation = Quaternion.Euler(projectileOrientation.eulerAngles.x, projectileOrientation.eulerAngles.y
                , projectileOrientation.eulerAngles.z - 90f);
            
            if (hitEffect != null && _config != null)
            {Debug.Log("projectileOrientation:" + projectileOrientation);
                for (int FX = 0; FX < _config.wallhitEffect.Count; FX++)
                {
                    
                    VisualEffect hitFX = _config.wallhitEffect[FX];
                    hitEffect.Apply(hitFX, other.transform.position, projectileOrientation);
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
                    hitEffectPS.Apply(hitFX, other.transform.position, projectileOrientation);
                    Debug.Log("hit PS Applied"+ hitFX.name);
                }
                
            }
            else if (hitEffect == null || _config == null)
            {
                Debug.LogWarning("hit effect/config is null");
            }
            
            Despawn(); 
        }
        
        if (_config == null) return;

        // Optional layer mask filter
        if (((1 << other.gameObject.layer) & _config.hitMask.value) == 0)
            return;

        // Don’t hit owner
        if (_owner != null && other.gameObject == _owner)
            return;

        // Friendly fire rule (team check requires target has Health/IDamageable with a team)
        var damageable = other.GetComponentInParent<IDamageable>();
        if (damageable == null)
            return;

        if (_config.preventFriendlyFire && damageable.Team == _ownerTeam)
            return;

        Debug.Log($"[CFG CHECK] cfgName={_config.name} cfgID={_config.GetInstanceID()} path={UnityEditor.AssetDatabase.GetAssetPath(_config)}");

        for (int i = 0; i < _config.hitEffectPS.Count; i++)
        {
            var ps = _config.hitEffectPS[i];
            Debug.Log($"[CFG hitEffectPS] i={i} val={(ps ? ps.name : "NULL")}");
        }
        for (int i = 0; i < _config.hitEffect.Count; i++)
        {
            var vfx = _config.hitEffect[i];
            Debug.Log($"[CFG hitEffect VFX] i={i} val={(vfx ? vfx.name : "NULL")}");
        }
        
        if (!other.CompareTag("Enemy")) return;
        
        _modifierSet?.NotifyHitEnemy(_shotAmmoType, _owner, other.gameObject, other.transform.position, other.transform.rotation);
        
        if (hitEffect != null && _config != null)
            {

                for (int FX = 0; FX < _config.hitEffect.Count; FX++)
                {
                    VisualEffect hitFX = _config.hitEffect[FX];
                    hitEffect.Apply(hitFX, other.transform.position, other.transform.rotation);
                    Debug.Log("hitEffect Applied" + hitFX.name);
                }
                
            }
            else if (hitEffect == null || _config == null)
            {
                Debug.LogWarning("hit effect/config is null");
            }

            if (hitEffectPS != null && _config != null)
            {
                for (int FX = 0; FX < _config.hitEffectPS.Count; FX++)
                {
                    ParticleSystem hitFX = _config.hitEffectPS[FX];
                    Debug.Log($"[ENEMY HIT PS] cfg={_config.name} index={FX} ps={(hitFX ? hitFX.name : "NULL")}");
                    if (hitFX == null) continue;
                    hitEffectPS.Apply(hitFX, other.transform.position, other.transform.rotation);
                    Debug.Log("hit PS Applied" + hitFX.name);
                }
                
            }
            else if (hitEffect == null || _config == null)
            {
                Debug.LogWarning("hit effect/config is null");
            }

            Debug.Log("HitEffect Applied");

       /* if (_mods != null)
        {
            for (int i = 0; i < _mods.Count; i++)
                _mods[i].OnHit(other.gameObject, _owner.gameObject);
           // Debug.Log($"OnHit target: {other.gameObject.name}"+ "has been slowed");
        }
     
            
        /*
        if(impactEffect)
            Instantiate(impactEffect, transform.position, Quaternion.identity);
Debug.LogError("HitEffect NULL");*/
        damageable.TakeDamage(_config.damage, _owner);

        if (_config.destroyOnHit)
        {
            Despawn();
            return;
        }

        // Piercing logic (optional)
        if (_remainingPierce > 0)
        {
            _remainingPierce--;
            if (_remainingPierce <= 0)
                Despawn();
        }
        else
        {
            // If not piercing and not destroyOnHit, you might still want to despawn.
            // Keep minimal default:
            Despawn();
        }
    }



    public void Despawn()
    {/*
        transform.localScale = Vector3.one;
        OutlineMaterialRenderer.GetPropertyBlock(mpb);
        mpb.Clear();
        OutlineMaterialRenderer.SetPropertyBlock(mpb);
       */
        
        // Stop physics motion to avoid “ghost velocity” on reuse
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        _owner = null;
        _config = null;

            gameObject.SetActive(false);
    }
}
