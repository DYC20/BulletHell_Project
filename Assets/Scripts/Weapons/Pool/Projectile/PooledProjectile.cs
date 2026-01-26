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
    [SerializeField] private ProjectileShootEffect shootEffect;
    
    [SerializeField]  private Renderer OutlineMaterialRenderer;
    private MaterialPropertyBlock mpb;

    private ProjectilePool _pool;

    // Runtime state
    private GameObject _owner;
    private Teams _ownerTeam;
    
    private ProjectileConfigSO _config;
    private ProjectileStats _stats;
    private List<ProjectileModifierSO> _mods;
    

    private int _remainingPierce;
    private float _lifeTimer;

    private void Awake()
    {
        
        if (OutlineMaterialRenderer == null) OutlineMaterialRenderer = GetComponent<Renderer>();
        mpb ??= new MaterialPropertyBlock();
    }

    private void Reset()
    {
        OutlineMaterialRenderer = GetComponent<Renderer>();
        rb = GetComponent<Rigidbody2D>();
        kinematics = GetComponent<ProjectileMovement>();
        hitEffect = GetComponent<ProjectileHitEffect>();
        shootEffect = GetComponent<ProjectileShootEffect>();
    }

    private void OnEnable()
    {
        // Important: ensure collider works immediately, but timer resets on Init
        _lifeTimer = 0f;
    }

    public void AssignPool(ProjectilePool pool) => _pool = pool;

    public void Init(GameObject owner, Teams ownerTeam, ProjectileConfigSO config, Vector2 direction, float speedOverride, Transform spawnTf,
        IReadOnlyList<ProjectileModifierSO> modifiers)
    {
        _owner = owner;
        _ownerTeam = ownerTeam;
        _config = config;
        _remainingPierce = config != null ? config.pierceCount : 0;
        //Set Modifiers
        _stats = ProjectileStatsBuilder.FromConfig(config);
        _mods = modifiers != null ? new List<ProjectileModifierSO>(modifiers) : null;
        Color _baseColor = OutlineMaterialRenderer.material.color;
        
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

        _lifeTimer = 0f;

        float speed = speedOverride > 0f ? speedOverride : config.speed;
        
        if (kinematics != null)
            kinematics.Apply(rb, config, direction, speed);
        else if (rb != null && config != null)
            rb.linearVelocity = direction.normalized * config.speed;
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg-90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        if (shootEffect != null && _config != null && spawnTf != null)
        {
            shootEffect.Apply(_config.shootEffect, spawnTf.position, spawnTf.rotation);
            Debug.Log("ShootEffect Applied"+_owner.name);
        }
        else if (shootEffect == null || _config == null || spawnTf == null)
        {
            Debug.LogWarning("shoot effect/config/spawnTF is null");
        }
    }

    private void Update()
    {
        if (_config == null) return;
        
        Vector3 startSize = Vector3.one;
        Vector3 endSize = Vector3.zero;

        _lifeTimer += Time.deltaTime;
        float t = (_config.lifetime <= 0f) ? 1f : Mathf.Clamp01(_lifeTimer / _config.lifetime);
        transform.localScale = Vector3.Lerp(startSize, endSize, t);
        if (_lifeTimer >= _config.lifetime)
            Despawn();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Walls"))
        {
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

        if (hitEffect != null)
        {
            hitEffect.Apply(_config.impactEffect, other.transform.position, other.transform.rotation);
            Debug.Log("HitEffect Applied");
        }

        if (_mods != null)
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
    {
        transform.localScale = Vector3.one;
        OutlineMaterialRenderer.GetPropertyBlock(mpb);
        mpb.Clear();
        OutlineMaterialRenderer.SetPropertyBlock(mpb);
        
        // Stop physics motion to avoid “ghost velocity” on reuse
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        _owner = null;
        _config = null;

        if (_pool != null)
            _pool.Return(this);
        else
            gameObject.SetActive(false);
    }
}
