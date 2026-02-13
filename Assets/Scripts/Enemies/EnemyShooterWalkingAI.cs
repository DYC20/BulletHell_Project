using System.Collections;
using UnityEngine;

public class EnemyShooterWalkingAI : MonoBehaviour
{
    private enum State { Wander, CombatChase, CombatReposition, CombatFiring }

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform firePoint;
    [SerializeField] private WeaponBase weapon;
    [SerializeField] private Teams enemyTeam = Teams.Enemy;
    
    [Header("Aiming")]
    [SerializeField] private Transform weaponPivot; // the transform you want to rotate (weapon root / arm / gun)
    [SerializeField] private bool aimWithFirePointUp = true; // assumes firePoint.up is the shoot direction

    [Header("Line of Sight")]
    //[SerializeField] private LayerMask walls;          // walls/cover layers (NOT player)
    [SerializeField] private float losRepositionRadius = 2f; // where to move if LOS is blocked
    [SerializeField] private float losSquareSize = 0.2f;     // debug
    [SerializeField] private bool drawLosDebug = true;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private LayerMask playerLayer; // optional, used for OverlapCircle
    private Transform player;

    [Header("Walls")]
    [SerializeField] private LayerMask wallLayer;

    [Header("Wander")]
    [SerializeField] private float wanderRadius = 4f;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float arriveDistance = 0.15f;
    [SerializeField] private float wanderWaitMin = 0.25f;
    [SerializeField] private float wanderWaitMax = 1.25f;

    [Header("Detection / Combat")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float firingRange = 6f;
    [SerializeField] private float stopDistanceInRange = 4.5f; // how close it tries to be when firing
    [SerializeField] private float repathIfBlockedCooldown = 0.15f;

    [Header("Firing")]
    [SerializeField] private float fireRateMin = 0.15f; // seconds between shots
    [SerializeField] private float fireRateMax = 0.45f;

    [Header("Reposition (after first shot)")]
    [SerializeField] private float repositionRadius = 2.5f; // radius around cached combat anchor
    [SerializeField] private float repositionIntervalMin = 1.5f;
    [SerializeField] private float repositionIntervalMax = 3.5f;
    [SerializeField] private float repositionArriveDistance = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;

    private State state;

    private Vector2 startPos;
    private Vector2 combatAnchorPos;
    private bool combatAnchorSet;

    private Vector2 currentTarget;
    private Coroutine brainRoutine;

    private float lastBlockedRepathTime;
    private bool forceReposition;
    private Transform playerAimPos;


    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;
        PickNewWanderTarget();
        
        if (weapon != null)
        {
            weapon.SetOwner(gameObject, enemyTeam);

            // ✅ CRITICAL: WeaponBase.TryFire uses weapon.firePoint (inside the weapon)
            if (weapon.FirePoint == null)
            {
                Debug.LogError($"{name}: Weapon '{weapon.name}' has NO firePoint assigned. Assign it on the weapon component (WeaponBase) in the Inspector.");
            }
        }
        else
        {
            Debug.LogError($"{name}: Missing weapon reference on EnemyShooterWalkingAI.");
        }
    }

    private void OnEnable()
    {
        brainRoutine = StartCoroutine(BrainLoop());
    }

    private void OnDisable()
    {
        if (brainRoutine != null) StopCoroutine(brainRoutine);
    }

    private void FixedUpdate()
    {
        // Move depending on state
        switch (state)
        {
            case State.Wander:
            case State.CombatChase:
            case State.CombatReposition:
                MoveTowards(currentTarget, repositionArriveDistance);
                break;

            case State.CombatFiring:
                // optional: stay still while firing
                rb.linearVelocity = Vector2.zero;
                break;
        }
    }
    
    private void Update()
    {
        if (player != null)
        {
            AimWeaponAtPlayer();
            DrawAimDebug(firingRange, 0.2f);
        }
    }

    private IEnumerator BrainLoop()
    {
        state = State.Wander;

        while (true)
        {
            // 1) Try detect player
            if (player == null)
                TryAcquirePlayer();

            bool playerDetected = player != null && Vector2.Distance(transform.position, player.position) <= detectionRadius;

            if (playerDetected)
            {
                // Combat flow
                yield return CombatTick();
            }
            else
            {
                // Wander flow
                yield return WanderTick();
            }

            yield return null;
        }
    }

    // -------------------------
    // Wander
    // -------------------------
    private IEnumerator WanderTick()
    {
        state = State.Wander;

        // If reached target, wait, then pick new target around START position
        if (HasArrived(currentTarget, arriveDistance))
        {
            rb.linearVelocity = Vector2.zero;

            float wait = Random.Range(wanderWaitMin, wanderWaitMax);
            yield return new WaitForSeconds(wait);

            PickNewWanderTarget();
        }

        yield return null;
    }

    private void PickNewWanderTarget()
    {
        currentTarget = RandomPointInRadius(startPos, wanderRadius);
    }

    // -------------------------
    // Combat
    // -------------------------
    private IEnumerator CombatTick()
    {
        // Stay in combat as long as player is detected.
        yield return CombatLoop();
    }
    
    private void AimWeaponAtPlayer()
    {
        if (weaponPivot == null) return;

        Transform target = playerAimPos != null ? playerAimPos : player;
        if (target == null) return;

        Vector2 dir = (target.position - weaponPivot.position);
        if (dir.sqrMagnitude < 0.0001f) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // firePoint.up is forward → compensate for sprite orientation
        float offset = aimWithFirePointUp ? -90f : 0f;

        weaponPivot.rotation = Quaternion.Euler(0f, 0f, angle + offset);
    }

    private bool HasLineOfSightToPlayer(out RaycastHit2D hit)
    {
        hit = default;

        if (weapon == null || weapon.FirePoint == null || player == null)
            return false;

        Vector2 origin = weapon.FirePoint.position;
        Vector2 target = playerAimPos != null
            ? (Vector2)playerAimPos.position
            : (Vector2)player.position;
        Vector2 dir = target - origin;

        float dist = dir.magnitude;
        if (dist <= 0.001f) return true;

        dir /= dist;

        // Cast ONLY against blockers. If it hits something, LOS is blocked.
        hit = Physics2D.Raycast(origin, dir, dist, wallLayer);

        if (drawLosDebug)
        {
            // green if clear, red if blocked
            Debug.DrawLine(origin, target, hit.collider ? Color.red : Color.green, 0.1f);
        }

        return hit.collider == null;
    }
    private void RepositionForLineOfSight()
    {
        state = State.CombatReposition;

        Vector2 center = combatAnchorSet ? combatAnchorPos : (Vector2)transform.position;
        currentTarget = RandomPointInRadius(center, Mathf.Max(losRepositionRadius, 0.1f));
    }

    private IEnumerator CombatLoop()
    {
        state = State.CombatChase;

        float nextShotTime = Time.time;
        float nextRepositionTime = Time.time + Random.Range(repositionIntervalMin, repositionIntervalMax);

        // When we had to chase (player left range), we want to refresh the anchor
        bool needsAnchorRefresh = !combatAnchorSet;

        while (player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);

            // If player is no longer "detected", exit combat
            // (uses your detectionRadius rule)
            if (dist > detectionRadius)
            {
                state = State.Wander;
                PickNewWanderTarget();
                yield break;
            }

            // --- CHASE LOGIC ---
            // If player is outside firingRange OR outside your preferred stopDistance,
            // chase until we can shoot.
            if (dist > firingRange || dist > stopDistanceInRange)
            {
                state = State.CombatChase;
                currentTarget = (Vector2)player.position;

                // We chased -> when we can shoot again, anchor should be refreshed
                needsAnchorRefresh = true;

                yield return null;
                continue;
            }

            // --- IN GOOD FIRING POSITION ---
            state = State.CombatFiring;
            rb.linearVelocity = Vector2.zero;

            // Refresh anchor once after chase / re-entering firing position
            if (needsAnchorRefresh)
            {
                combatAnchorSet = true;
                combatAnchorPos = transform.position;

                // Reset reposition timer so we don't instantly reposition right after re-entering
                nextRepositionTime = Time.time + Random.Range(repositionIntervalMin, repositionIntervalMax);

                needsAnchorRefresh = false;
            }

            // --- REPOSITION ---
            if (forceReposition || combatAnchorSet && Time.time >= nextRepositionTime)
            {
                state = State.CombatReposition;
                
                Vector2 center = combatAnchorSet ? combatAnchorPos : (Vector2)transform.position;
                float radius = forceReposition ? losRepositionRadius : repositionRadius;

                currentTarget = RandomPointInRadius(combatAnchorPos, repositionRadius);
                
                forceReposition = false;
                nextRepositionTime = Time.time + Random.Range(repositionIntervalMin, repositionIntervalMax);

                // Walk to reposition target, but don't leave combat loop
                while (!HasArrived(currentTarget, repositionArriveDistance))
                {
                    // If player vanished, exit
                    if (player == null) yield break;

                    dist = Vector2.Distance(transform.position, player.position);

                    // If player moved far away, stop reposition and chase instead
                    if (dist > firingRange || dist > stopDistanceInRange)
                    {
                        needsAnchorRefresh = true;
                        break;
                    }

                    yield return null;
                }

                // Plan next reposition time
               
                rb.linearVelocity = Vector2.zero;
                state = State.CombatFiring;

                yield return null;
                continue;
            }

            // --- SHOOT ---
            if (Time.time >= nextShotTime)
            {
                // 1) LOS check before the shot
                if (!HasLineOfSightToPlayer(out RaycastHit2D losHit))
                {
                    // blocked -> reposition instead of shooting
                    forceReposition = true;

                    // Optional: small delay so it doesn't spam checks every frame
                    nextShotTime = Time.time + 0.15f;
                    yield return null;
                    continue;
                }

                // 2) Clear LOS -> try shoot
                if (weapon != null)
                {
                    bool fired = weapon.TryFire();

                    // schedule next shot regardless
                    float delay = Random.Range(fireRateMin, fireRateMax);
                    nextShotTime = Time.time + delay;

                    if (fired)
                    {
                        if (!combatAnchorSet)
                        {
                            combatAnchorSet = true;
                            combatAnchorPos = transform.position;
                        }
                    }
                }
            }
            yield return null;
        }
    }

    // -------------------------
    // Player detection
    // -------------------------
    private void TryAcquirePlayer()
    {
        // Try cheap overlap first (optional)
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);
        if (hit != null && hit.CompareTag(playerTag))
        {
            player = hit.transform;
            
            playerAimPos = player.Find("AimPos");
            if (playerAimPos == null)
            {
                Debug.LogWarning($"{name}: Player has no child named 'AimPos'. Falling back to player transform.");
            }

            return;
        }

        // Fallback: Find by tag (only if needed, not every frame)
        GameObject go = GameObject.FindGameObjectWithTag(playerTag);
        if (go)
        {
            player = go.transform;
            playerAimPos = player.Find("AimPos");
        }
    }

    // -------------------------
    // Movement helpers
    // -------------------------
    private void MoveTowards(Vector2 target, float stopThreshold)
    {
        Vector2 pos = rb.position;
        Vector2 dir = (target - pos);

        if (dir.sqrMagnitude <= stopThreshold * stopThreshold)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        dir.Normalize();

        // Simple movement
        rb.linearVelocity = dir * moveSpeed;
    }

    private bool HasArrived(Vector2 target, float threshold)
    {
        return Vector2.Distance(transform.position, target) <= threshold;
    }

    private Vector2 RandomPointInRadius(Vector2 center, float radius)
    {
        // Uniform random inside circle
        Vector2 offset = Random.insideUnitCircle * radius;
        
        return center + offset;
    }

    // -------------------------
    // Wall collision -> repick wander target immediately
    // -------------------------
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If we hit a wall, repath depending on current mode
        if (((1 << collision.gameObject.layer) & wallLayer) == 0)
            return;

        // Prevent spam if physics contacts fire a lot
        if (Time.time - lastBlockedRepathTime < repathIfBlockedCooldown)
            return;

        lastBlockedRepathTime = Time.time;

        if (state == State.Wander)
        {
            PickNewWanderTarget();
        }
        else if (state == State.CombatReposition)
        {
            // Try a different reposition point
            currentTarget = combatAnchorSet
                ? RandomPointInRadius(combatAnchorPos, repositionRadius)
                : (Vector2)player.position;
        }
        else if (state == State.CombatChase)
        {
            // Try to still chase player; this is simple, you can improve with pathfinding later
            if (player) currentTarget = (Vector2)player.position;
        }
    }

    // -------------------------
    // Gizmos
    // -------------------------
    /*private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        Gizmos.DrawWireSphere(Application.isPlaying ? (Vector3)startPos : transform.position, wanderRadius);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (Application.isPlaying && combatAnchorSet)
            Gizmos.DrawWireSphere(combatAnchorPos, repositionRadius);
    }*/
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        // Wander radius
        Gizmos.color = Color.cyan;
        Vector3 wanderCenter = startPos;
        Gizmos.DrawWireSphere(wanderCenter, wanderRadius);

        // Detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Firing range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, firingRange);

        // Current target
        Gizmos.color = Color.magenta;
        DrawGizmoSquare(currentTarget, 0.2f);

        // Combat anchor
        if (combatAnchorSet)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawWireSphere(combatAnchorPos, repositionRadius);
        }
        
        if (!Application.isPlaying || playerAimPos == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(playerAimPos.position, 0.15f);
    }

    private void DrawGizmoSquare(Vector2 center, float size)
    {
        Vector3 a = center + new Vector2(-size, -size);
        Vector3 b = center + new Vector2(-size,  size);
        Vector3 c = center + new Vector2( size,  size);
        Vector3 d = center + new Vector2( size, -size);

        Gizmos.DrawLine(a, b);
        Gizmos.DrawLine(b, c);
        Gizmos.DrawLine(c, d);
        Gizmos.DrawLine(d, a);
    }
    private void DrawAimDebug(float distance, float radius)
    {
        if (weapon == null || weapon.FirePoint == null) return;

        Vector2 origin = weapon.FirePoint.position;
        Vector2 aimDir = weapon.FirePoint.up;
        Vector2 aimPoint = origin + aimDir * distance;

        DrawDebugCircle(aimPoint, radius, Color.cyan, 0.1f);
        Debug.DrawLine(origin, aimPoint, Color.cyan, 0.1f);
    }
    private void DrawDebugCircle(Vector2 center, float radius, Color color, float duration)
    {
        const int steps = 24;
        Vector3 prev = center + Vector2.right * radius;

        for (int i = 1; i <= steps; i++)
        {
            float a = i * Mathf.PI * 2f / steps;
            Vector3 next = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
            Debug.DrawLine(prev, next, color, duration);
            prev = next;
        }
    }

}
