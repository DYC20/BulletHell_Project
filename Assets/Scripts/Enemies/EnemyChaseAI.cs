using UnityEngine;

public class EnemyChaseAI : MonoBehaviour, IEnemyMoveSpeed
{
    [Header("Directional Sprites (4-way)")]
    [SerializeField] private SpriteRenderer renderer; // assign in Inspector
    [SerializeField] private Sprite spriteLeft;
    [SerializeField] private Sprite spriteRight;
    
    [Header("Target")]
    [SerializeField] private Transform player;           // assign in Inspector (recommended)
    [SerializeField] private string playerTag = "Player"; // fallback if not assigned
                                  
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float ratationSpeed = 3.5f;
    private RigidbodyType2D lastBodyType;
    
    public bool isEnemyGrounded {get; private set; }
    public float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = Mathf.Max(0f, value);
    }
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
        animator = GetComponent<Animator>();
        // If player not set, try find by tag
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) player = p.transform;
        }

        //CachePlayerHealth();
    }

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
            Debug.Log("Linear velocity: " + rb.linearVelocity);
            Debug.Log("ai Enabled: " + aiEnabled);
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

        
        // Move toward player using Rigidbody2D (physics-friendly)
        Vector2 toPlayer = ((Vector2)player.position - rb.position);
        Vector2 dir = toPlayer.sqrMagnitude > 0.0001f ? toPlayer.normalized : Vector2.zero;

        rb.linearVelocity = dir * moveSpeed;
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
}
