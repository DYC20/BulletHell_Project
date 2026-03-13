using UnityEngine;

public class EnemyShooterAI : MonoBehaviour, IEnemyFireInterval
{

    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private string targetTag = "Player";

    [Header("Weapon")]
    [SerializeField] private WeaponBase weapon;
    [SerializeField] private Transform firePoint;

    [Header("Shooting Rules")]
    [SerializeField] private float range = 8f;
    [SerializeField] private float fireInterval = 0.6f;
    [SerializeField] private LayerMask lineOfSightMask;
    [SerializeField] private bool requireLineOfSight = true;

    [Header("Aim")]
    [SerializeField] private float aimTurnSpeed = 999f;

    private float nextFireTime;
    public bool isEnemyGrounded { get; private set; }

    private Rigidbody2D rb;

    private enum AIState
    {
        Disengaged,
        Engaged
    }

    private AIState currentState = AIState.Disengaged;

    [SerializeField] private int patrolDirection = 1; // 1 = right, -1 = left
    [SerializeField] private bool isWaitingAtWall;
    [SerializeField] private float waitTimer;

    [SerializeField] private const float patrolSpeed = 2f;
    [SerializeField] private const float wallWaitDuration = 2f;

    // engaged movement tuning
    [SerializeField] private const float engageMoveSpeed = 2.5f;
    [SerializeField] private const float preferredRangeOffset = 1.5f; // tries to stay near range - offset
    [SerializeField] private const float rangeTolerance = 0.5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (weapon == null) weapon = GetComponentInChildren<WeaponBase>();
        if (firePoint == null && weapon != null)
        {
            firePoint = weapon.FirePoint;
        }

        isEnemyGrounded = false;
    }

    private void Start()
    {
        if (target == null)
        {
            var go = GameObject.FindGameObjectWithTag(targetTag);
            if (go != null) target = go.transform;
        }

        if (weapon != null)
            weapon.SetOwner(gameObject, Teams.Enemy);
    }

    public void SetEnemyGrounded(bool value)
    {
        isEnemyGrounded = value;
    }

    private void Update()
    {
        if (weapon == null || target == null) return;

        Vector2 toTarget = target.position - transform.position;
        float distSqr = toTarget.sqrMagnitude;
        float dist = Mathf.Sqrt(distSqr);

        // state switch
        currentState = dist <= range ? AIState.Engaged : AIState.Disengaged;

        HandleMovement(dist, toTarget);

        HandleAim(toTarget, distSqr);

        if (distSqr > range * range) return;

        if (requireLineOfSight && !HasLineOfSight())
            return;

        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireInterval;
            weapon.TryFire();
        }
    }

    private void HandleMovement(float dist, Vector2 toTarget)
    {
        switch (currentState)
        {
            case AIState.Disengaged:
                HandleDisengagedMovement();
                break;

            case AIState.Engaged:
                HandleEngagedMovement(dist, toTarget);
                break;
        }
    }

    private void HandleDisengagedMovement()
    {
        if (isWaitingAtWall)
        {
            waitTimer -= Time.deltaTime;
            rb.linearVelocity = Vector2.zero;

            if (waitTimer <= 0f)
            {
                isWaitingAtWall = false;
                patrolDirection *= -1;
            }

            return;
        }

        rb.linearVelocity = new Vector2(patrolDirection * patrolSpeed, 0f);
    }

    private void HandleEngagedMovement(float dist, Vector2 toTarget)
    {
        isWaitingAtWall = false;

        float preferredRange = range - preferredRangeOffset;
        float minPreferred = preferredRange - rangeTolerance;
        float maxPreferred = preferredRange + rangeTolerance;

        Vector2 dir = toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized : Vector2.zero;

        if (dist > maxPreferred)
        {
            // too far -> move toward player
            rb.linearVelocity = new Vector2(dir.x * engageMoveSpeed, 0f);
        }
        else if (dist < minPreferred)
        {
            // too close -> move away from player
            rb.linearVelocity = new Vector2(-dir.x * engageMoveSpeed, 0f);
        }
        else
        {
            // good distance -> stop
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void HandleAim(Vector2 toTarget, float distSqr)
    {
        if (distSqr > range * range) return;
        if (firePoint == null) return;

        Vector2 dir = toTarget.normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        weapon.transform.rotation = Quaternion.RotateTowards(
            weapon.transform.rotation,
            Quaternion.Euler(0f, 0f, angle),
            aimTurnSpeed * Time.deltaTime
        );
    }

    private bool HasLineOfSight()
    {
        Vector2 origin = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
        Vector2 dir = ((Vector2)target.position - origin).normalized;
        float dist = Vector2.Distance(origin, target.position);

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, dist, lineOfSightMask);
        return hit.collider == null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState != AIState.Disengaged) return;
        if (isWaitingAtWall) return;

        isWaitingAtWall = true;
        waitTimer = wallWaitDuration;
        rb.linearVelocity = Vector2.zero;
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

    public float FireInterval
    {
        get => fireInterval;
        set => fireInterval = Mathf.Max(0.02f, value);
    }

}

/*
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
public bool isEnemyGrounded {get; private set; }

private void Awake()
{
    if (weapon == null) weapon = GetComponentInChildren<WeaponBase>();
    if (firePoint == null && weapon != null)
    {
        // If WeaponBase exposes firePoint, use it. Otherwise assign in inspector.
        firePoint = weapon.FirePoint;
    }
    isEnemyGrounded = false;
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

public void SetEnemyGrounded(bool value)
{
    isEnemyGrounded = value;
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
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg ; // because up is forward

        weapon.transform.rotation = Quaternion.RotateTowards(
            weapon.transform.rotation,
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
public float FireInterval
{
    get => fireInterval;
    set => fireInterval = Mathf.Max(0.02f, value);

}
}*/