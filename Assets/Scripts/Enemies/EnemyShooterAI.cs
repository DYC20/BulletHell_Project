using UnityEngine;

public class EnemyShooterAI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private string targetTag = "Player";

    [Header("Weapon")]
    [SerializeField] private WeaponBase weapon;      // assign (or auto-find)
    [SerializeField] private Transform firePoint;    // assign (or use weapon's firePoint)

    [Header("Shooting Rules")]
    [SerializeField] private float range = 8f;
    [SerializeField] private float fireInterval = 0.6f; // seconds
    [SerializeField] private LayerMask lineOfSightMask; // what blocks vision (Walls, etc.)
    [SerializeField] private bool requireLineOfSight = true;

    [Header("Aim")]
    [SerializeField] private float aimTurnSpeed = 999f; // for top-down instant aim

    private float nextFireTime;

    private void Awake()
    {
        if (weapon == null) weapon = GetComponentInChildren<WeaponBase>();
        if (firePoint == null && weapon != null)
        {
            // If WeaponBase exposes firePoint, use it. Otherwise assign in inspector.
            firePoint = weapon.FirePoint;
        }
    }

    private void Start()
    {
        if (target == null)
        {
            var go = GameObject.FindGameObjectWithTag(targetTag);
            if (go != null) target = go.transform;
        }

        // IMPORTANT: ensure the weapon knows its owner/team (depends on your WeaponBase design)
        // If your WeaponBase has a method like AssignOwner, call it here.
         weapon.SetOwner(gameObject, Teams.Enemy);
    }

    private void Update()
    {
        if (weapon == null || target == null) return;

        Vector2 toTarget = target.position - transform.position;
        float distSqr = toTarget.sqrMagnitude;
        if (distSqr > range * range) return;

        // Aim: rotate firePoint (or enemy) so firePoint.up points at target
        if (firePoint != null)
        {
            Vector2 dir = toTarget.normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f; // because up is forward
            weapon.transform.rotation = Quaternion.RotateTowards(
                firePoint.rotation,
                Quaternion.Euler(0, 0, angle),
                aimTurnSpeed * Time.deltaTime
            );
        }

        if (requireLineOfSight && !HasLineOfSight())
        {
            Debug.Log("Sniper !HasLineOfSight"); return;
        }

        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireInterval;

            // This depends on your WeaponBase API.
            // If WeaponBase has TryFire() / Fire() call it here.
            // Example:
            weapon.TryFire();
//            Debug.Log("Sniper Fired");
        }
    }

    private bool HasLineOfSight()
    {
        Vector2 origin = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
        Vector2 dir = ((Vector2)target.position - origin).normalized;
        float dist = Vector2.Distance(origin, target.position);

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, dist, lineOfSightMask);
        // If we hit something, LOS is blocked
        return hit.collider == null;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, range);
        if (target != null)
        {
            Vector3 a = firePoint != null ? firePoint.position : transform.position;
            Gizmos.DrawLine(a, target.position);
        }
    }
#endif
}