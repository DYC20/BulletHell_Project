using UnityEngine;

public class EnemyChaseAI : MonoBehaviour
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

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // typical for top-down 2D
    }

    private void Start()
    {
        
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
    private void FixedUpdate()
    {
        if (!aiEnabled ||player == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Move toward player using Rigidbody2D (physics-friendly)
        Vector2 toPlayer = ((Vector2)player.position - rb.position);
        Vector2 dir = toPlayer.sqrMagnitude > 0.0001f ? toPlayer.normalized : Vector2.zero;

        rb.linearVelocity = dir * moveSpeed;
        
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
                if (targetAngle >= -180f && targetAngle < 180f)              renderer.sprite = spriteRight;
                else                                   renderer.sprite = spriteLeft;
            }
        }
    }
    /*
    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryHitPlayer(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryHitPlayer(collision.collider);
    }
    

    private void TryHitPlayer(Collider2D other)
    {
        if (Time.time < nextTimeCanHit)
            return;

        // Fast path: tag check
        if (!other.CompareTag(playerTag))
            return;

        // Ensure we have the Health reference (in case player was found late)
        if (playerHealth == null)
        {
            playerHealth = other.GetComponent<Health>();
            if (playerHealth == null) return;
        }

        playerHealth.TakeDamage(contactDamage);
        nextTimeCanHit = Time.time + hitCooldown;

        Debug.Log("PlayerHealth"+playerHealth.currentHP);
    }
    */
}
