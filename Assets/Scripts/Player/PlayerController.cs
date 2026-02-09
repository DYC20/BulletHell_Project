using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 6f;
    private Animator animator;

    [Header("Movement Behavior")]
    public float accel = 60f;
    public float decel = 80f;
    public float turnBoost = 1.5f;

    [Header("Aiming")]
    [SerializeField] private Transform weaponHolder;   // assign in Inspector
    [SerializeField] private float holderRotationSpeed = 25f; // visual smoothing
/*
    [Header("Directional Sprites (4-way)")]
    [SerializeField] private SpriteRenderer playerRenderer; // assign in Inspector
    [SerializeField] private Sprite spriteUp;
    [SerializeField] private Sprite spriteRight;
    [SerializeField] private Sprite spriteDown;
    [SerializeField] private Sprite spriteLeft;
*/
    private Vector2 move;
    private Vector2 mouseScreenPos;

    private Camera cam;
    private Rigidbody2D rb;

    private void Awake()
    {
        cam = Camera.main;

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true; // keep the root stable
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        animator = GetComponent<Animator>();
        animator.SetBool("isIdle", true);
       
    }

    public void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();
        if (move.sqrMagnitude > 1f) move.Normalize();
        if (move.x != 0 || move.y != 0)
        {
            animator.SetBool("isWalking", true);
            animator.SetBool("isIdle", false);
            if (move.x > 0) gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
            if (move.x < 0) gameObject.transform.localScale = new Vector3(-1f, 1f, 1f);
        }
        else
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isIdle", true);
        }
       
    }

    public void OnLook(InputValue value)
    {
        mouseScreenPos = value.Get<Vector2>();
    }

    private void FixedUpdate()
    {
        // MOVE (physics-based)
        Vector2 targetVel = move * speed;

        float rate;
        if (move == Vector2.zero)
        {
            rate = decel;
        }
        else
        {
            rate = accel;
            if (Vector2.Dot(rb.linearVelocity, targetVel) < 0f)
                rate *= turnBoost;
        }

        rb.linearVelocity = Vector2.MoveTowards(
            rb.linearVelocity,
            targetVel,
            rate * Time.fixedDeltaTime
        );
    }

    private void Update()
    {
        if (cam == null) return;

        // Mouse world point
        Vector3 mouseWorld3 = cam.ScreenToWorldPoint(new Vector3(
            mouseScreenPos.x,
            mouseScreenPos.y,
            -cam.transform.position.z
        ));

        Vector2 mouseWorld = mouseWorld3;
        DrawDebugCircle(mouseWorld, 0.2f, Color.green, 0f);
      


        Vector2 toMouse = mouseWorld - rb.position;

        if (toMouse.sqrMagnitude < 0.0001f)
            return;

        // 1) Rotate ONLY the weapon holder toward mouse
        if (weaponHolder != null)
        {
            float angle = Mathf.Atan2(toMouse.y, toMouse.x) * Mathf.Rad2Deg -90f;
            Quaternion targetRot = Quaternion.Euler(0f, 0f, angle);

            weaponHolder.rotation = Quaternion.Slerp(
                weaponHolder.rotation,
                targetRot,
                holderRotationSpeed * Time.deltaTime
            );
        }

        // 2) Swap player sprite based on mouse direction (4-way)
       
            SetSpriteFromDirection(toMouse);
        
    }

    private void SetSpriteFromDirection(Vector2 dir)
    {
        // angle where: right=0, up=90, left=180/-180, down=-90
        float a = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // 4-way thresholds at 45 degrees
        if (a >= -45f && a < 45f)              gameObject.transform.localScale.Set(1f, 1f, 1f);
        else if (a >= 45f && a < 135f)         gameObject.transform.localScale.Set(1f, 1f, 1f);
        else if (a >= -135f && a < -45f)       gameObject.transform.localScale.Set(-1f, 1f, 1f);
        else                                   gameObject.transform.localScale.Set(-1f, 1f, 1f);
    }
    private void DrawDebugCircle(Vector2 center, float radius, Color color, float duration)
    {
        const int segments = 24;
        Vector3 prev = center + Vector2.right * radius;

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            Vector3 next = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            Debug.DrawLine(prev, next, color, duration);
            prev = next;
        }
    }

}

