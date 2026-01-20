using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(Collider2D))]
public class PooledProjectile : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private ProjectileMovement kinematics;
    [SerializeField] private ProjectileHitEffect hitEffect;
    [SerializeField] private ProjectileShootEffect shootEffect;


    private ProjectilePool _pool;

    // Runtime state
    private GameObject _owner;
    private Teams _ownerTeam;
    private ProjectileConfigSO _config;
    private int _remainingPierce;

    private float _lifeTimer;

    private void Reset()
    {
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

    public void Init(GameObject owner, Teams ownerTeam, ProjectileConfigSO config, Vector2 direction, Transform spawnTf)
    {
        _owner = owner;
        _ownerTeam = ownerTeam;
        _config = config;
        _remainingPierce = config != null ? config.pierceCount : 0;

        _lifeTimer = 0f;

        if (kinematics != null)
            kinematics.Apply(rb, config, direction);
        else if (rb != null && config != null)
            rb.linearVelocity = direction.normalized * config.speed;

        if (shootEffect != null && _config != null && spawnTf != null)
        {
            shootEffect.Apply(_config.shootEffect, spawnTf.position, spawnTf.rotation);
            Debug.Log("ShootEffect Applied"+_owner.name);
        }
    }

    private void Update()
    {
        if (_config == null) return;

        _lifeTimer += Time.deltaTime;
        if (_lifeTimer >= _config.lifetime)
            Despawn();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
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
