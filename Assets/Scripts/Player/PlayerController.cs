using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private float rotationSpeed = 180f;
    
    [Header("Movement Behavior")]
    public float accel = 60f;        // how fast you reach speed
    public float decel = 80f;        // how fast you stop
    public float turnBoost = 1.5f;   // extra accel when reversing direction

    private Vector2 move;
    private Vector2 mouseScreenPos;
    
    private Camera cam;
    private Rigidbody2D rb;
    
    private void Awake()
    {
        cam = Camera.main;
        
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }
    
    // IMPORTANT: method name must match the action name: "Move" => "OnMove"
    public void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();
        
        if (move.sqrMagnitude > 1f)
            move.Normalize();
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
            rate = decel; // stopping
        }
        else
        {
            rate = accel; // starting / moving

            // If reversing direction, boost acceleration so it snaps
            if (Vector2.Dot(rb.linearVelocity, targetVel) < 0f)
                rate *= turnBoost;
        }

        rb.linearVelocity = Vector2.MoveTowards(
            rb.linearVelocity,
            targetVel,
            rate * Time.fixedDeltaTime
        );

        // ROTATE TOWARD MOUSE (stable world point)
        if (cam == null) return;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(
            mouseScreenPos.x,
            mouseScreenPos.y,
            -cam.transform.position.z // distance to z=0 plane (typical 2D)
        ));

        Vector2 toMouse = (Vector2)mouseWorld - rb.position;

        if (toMouse.sqrMagnitude > 0.0001f)
        {
            float targetAngle = Mathf.Atan2(toMouse.y, toMouse.x) * Mathf.Rad2Deg -90;

            // smooth rotation with fixed timestep
            float newAngle = Mathf.LerpAngle(rb.rotation, targetAngle, rotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(newAngle);
        }
        
    }
        //Debug.Log(transform.eulerAngles);
}
