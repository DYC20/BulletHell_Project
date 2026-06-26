using System.Collections;
using UnityEngine;

public class EnemyChaseAI : MonoBehaviour, IEnemyMoveSpeed, IWeaponPivot
{
    [Header("Directional Sprites (4-way)")]
    [SerializeField] private SpriteRenderer renderer; // assign in Inspector
    [SerializeField] private Sprite spriteLeft;
    [SerializeField] private Sprite spriteRight;
    
    [Header("Target")]
    [SerializeField] private Transform player;           // assign in Inspector (recommended)
    [SerializeField] private string playerTag = "Player"; // fallback if not assigned
    private Transform playerVisualCenter;
                                  
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float ratationSpeed = 3.5f;
    private RigidbodyType2D lastBodyType;
    private Vector2 toPlayer = new Vector2();
    private Vector2 dir = new Vector2();

    private Transform weaponPivot;
    private ContactDamage contactDamage;
    
    public bool isEnemyGrounded {get; private set; }
    public float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = Mathf.Max(0f, value);
    }
    
    private enum EnemyState
    {
        Chase,
        Retreat
    }

    private EnemyState state = EnemyState.Chase;

    [Header("Retreat / Circle Back")]
    [SerializeField] private float retreatSpeedMultiplier = 1.2f;
    [SerializeField] private float retreatAwayWeight = 0.7f;
    [SerializeField] private float retreatOrbitWeight = 1.0f;

    private float retreatEndTime;
    private int orbitDirection = 1;
    
/*  [Header("Contact Damage")]
    [SerializeField] private float contactDamage = 10f;
    [SerializeField] private float hitCooldown = 0.5f;   // seconds between hits while touching
*/
    private Rigidbody2D rb;
    //private Health playerHealth;
    //private float nextTimeCanHit;
    private bool aiEnabled;
    private Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        lastBodyType = rb.bodyType;
        rb.gravityScale = 0f; // typical for top-down 2D
        isEnemyGrounded = true;
    }

    private void Start()
    {
        contactDamage = GetComponent<ContactDamage>();
        if (contactDamage != null)
        {
            contactDamage.OnContactDamage += HandleDamageInflicted;
        }
        
        animator = GetComponent<Animator>();
        // If player not set, try find by tag
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag(playerTag);
            
            if (p != null) player = p.transform.root;
        }

        if (player == null)
        {
            Debug.LogWarning("No player found");
        }
        if (player != null)
        {
            Debug.LogWarning("Player: " + player.name);
            /*
            foreach (Transform child in player.GetComponentsInChildren<Transform>(true))
            {
                Debug.LogWarning("Child in hierarchy: " + GetFullPath(child));
            }
            */
            playerVisualCenter = player.Find("VisualCenter(DNCN)");
        //    Debug.LogWarning("Playe Visual Center: " + playerVisualCenter.name);
            if (playerVisualCenter == null) Debug.LogWarning("Player Visual Center is null");
        }
        //CachePlayerHealth();
    }
    
    private void OnDestroy()
    {
        if (contactDamage != null)
        {
            contactDamage.OnContactDamage -= HandleDamageInflicted;
        }
    }
    
    private void HandleDamageInflicted(GameObject damagedTarget)
    {
        state = EnemyState.Retreat;
        retreatEndTime = Time.time + contactDamage.HitCooldown;

        // Pick clockwise or counter-clockwise orbit direction.
        orbitDirection = Random.value < 0.5f ? -1 : 1;
    }
    
    //
    //player heirarchy debug
    //
    /*
    private string GetFullPath(Transform t)
    {
        string path = t.name;

        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }

        return path;
    }
    */
    public void SetAIEnabled(bool enabled)
    {
        aiEnabled = enabled;
        if (!aiEnabled && rb != null)
            rb.linearVelocity = Vector2.zero;
    }
    
    /* 
    private void CachePlayerHealth()
    {
        playerHealth = null;
        if (player != null)
            playerHealth = player.GetComponent<Health>();
    }
*/    

    public void SetEnemyGrounded(bool value)
    {
        isEnemyGrounded = value;
    }
    
    private void FixedUpdate()
    {
        if (!aiEnabled ||player == null)
        {
            rb.linearVelocity = Vector2.zero;
  //          Debug.Log("Linear velocity: " + rb.linearVelocity);
//            Debug.Log("ai Enabled: " + aiEnabled);
            if (player == null)
            {
                Debug.Log("Player is NULL");
            }

            return;
        }

        if (rb.bodyType != lastBodyType)
        {
            if (rb.bodyType == RigidbodyType2D.Static)
                    {
                        animator.speed = 0f;
                    }
            else if (rb.bodyType == RigidbodyType2D.Dynamic)
                    {
                        animator.speed = 1f;
                    }



            lastBodyType = rb.bodyType;
        }

        if (playerVisualCenter == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        
        // Move toward player using Rigidbody2D (physics-friendly)
        //Vector2 toPlayer = ((Vector2)player.position - rb.position);
        toPlayer = (Vector2)playerVisualCenter.position - rb.position;
        
        if (state == EnemyState.Retreat)
        {
            if (Time.time < retreatEndTime)
            {
                Retreat(toPlayer);
                return;
            }

            state = EnemyState.Chase;
        }
        
        Chase(toPlayer);
      
        Debug.Log("Linear velocity: " + rb.linearVelocity);
        Debug.Log("Body Type: " + rb.bodyType);
        
        if (toPlayer.sqrMagnitude > 0.0001f)
        {
            float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg -90;

            // smooth rotation with fixed timestep
           // float z = Mathf.LerpAngle(rb.rotation, targetAngle, ratationSpeed * Time.fixedDeltaTime);
            //rb.MoveRotation(z);
            
            if (renderer != null)
            {
                // angle where: right=0, up=90, left=180/-180, down=-90
               // float a = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                // 4-way thresholds at 45 degrees
                if (targetAngle >= -180f && targetAngle < 180f)
                {
                    this.transform.localScale = new Vector3(-1f, 1f, 1f);
                }
                else
                {
                    this.transform.localScale = new Vector3(1f, 1f, 1f);
                }
            }
        }
    }

    private void Chase(Vector2 toPlayer)
    {
        dir = toPlayer.sqrMagnitude > 0.0001f ? toPlayer.normalized : Vector2.zero;
        
        rb.linearVelocity = dir * moveSpeed;
    }
    
    private void Retreat(Vector2 toPlayer)
    {
        if (toPlayer.sqrMagnitude < 0.0001f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Direction from player to enemy
        Vector2 awayFromPlayer = -toPlayer.normalized;

        // Perpendicular direction, used to orbit around the player
        Vector2 orbitDirectionVector = new Vector2(
            -awayFromPlayer.y,
            awayFromPlayer.x
        ) * orbitDirection;

        // Blend away movement + orbit movement
        Vector2 retreatDir = 
            awayFromPlayer * retreatAwayWeight +
            orbitDirectionVector * retreatOrbitWeight;

        retreatDir.Normalize();

        rb.linearVelocity = retreatDir * moveSpeed * retreatSpeedMultiplier;
    }
    public Transform WeaponPivot
    {
        get => weaponPivot;
        set => weaponPivot = value;
    }
}
