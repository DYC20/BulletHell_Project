using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer playerVisual;
    private AudioSource audioSource;
    private AudioClip audioClip;

    [Header("Movement Behavior")]
    public float accel = 60f;
    public float decel = 80f;
    public float turnBoost = 1.5f;
    private bool isWalking;
    
    [Header("Movement Effects")]
    [SerializeField] private GameObject playerFX;
    [SerializeField] private Color walkingColorRoad;
    [SerializeField] private Color walkingColorGrass;

    private ParticleSystem walkingFX;
    private LayerMask roadLayer;

    [Header("Aiming")]
    [SerializeField] private Transform weaponHolder;   // assign in Inspector
    [SerializeField] private float holderRotationSpeed = 25f; // visual smoothing
    private Vector2 move;
    private Vector2 mouseScreenPos;

    private Camera cam;
    private Rigidbody2D rb;
    
    private Vector3 HoldOrigin;
    
    [Header("Ground Check")]
    public bool isPlayerGrounded {get; private set; }
    

    private void Awake()
    {
        cam = Camera.main;

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true; // keep the root stable
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        walkingFX = playerFX.transform.Find("Walking_SmokeTrail").gameObject.GetComponent<ParticleSystem>();
        if (walkingFX == null) Debug.Log("Player walking FX is null");
        if(walkingFX != null) Debug.Log("walking FX:" + walkingFX.name);
        
        if (playerVisual == null) playerVisual = GetComponentInChildren<SpriteRenderer>();
        roadLayer = LayerMask.GetMask("Road");

        animator.SetBool("isIdle", true);
        
        HoldOrigin = weaponHolder.transform.localPosition;

        isPlayerGrounded = true;
        
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource != null) audioClip = audioSource.clip;
    }

    public void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();
        if (move.sqrMagnitude > 1f) move.Normalize();
        if (move.x != 0 || move.y != 0)
        {
            isWalking = true;
            animator.SetBool("isWalking", true);
            animator.SetBool("isIdle", false);
            
            if (move.x > 0)
            {
                playerVisual.transform.localScale = new Vector3(1f, 1f, 1f);
                audioSource.PlayOneShot(audioClip);
                //playerFX.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }

            if (move.x < 0)
            {
                playerVisual.transform.localScale = new Vector3(-1f, 1f, 1f);
                audioSource.PlayOneShot(audioClip);
                //playerFX.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
        }
        else
        {
            isWalking = false;
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
        Vector2 toMouse = mouseWorld - rb.position;
        DrawDebugCircle(mouseWorld, 0.2f, Color.green, 0f);

        UpdateWalkingSurface();
        if (isWalking)
        {
            if (!walkingFX.isPlaying)
                walkingFX.Play();
        }
        else
        {
            if (walkingFX.isPlaying)
                walkingFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
/*
        Vector2 origin = GetComponent<PlayerWeaponController>()?.CurrentFirePoint != null
            ? (Vector2)GetComponent<PlayerWeaponController>().CurrentFirePoint.position
            : (weaponHolder != null ? (Vector2)weaponHolder.position : rb.position);

        Vector2 toMouse = mouseWorld - origin;
*/
       // Vector2 origin = weaponHolder != null ? (Vector2)weaponHolder.position : rb.position;
       // Vector2 toMouse = mouseWorld - origin;

        if (toMouse.sqrMagnitude < 0.0001f)
            return;

       // Vector2 aimDir = toMouse.normalized;
        
        // 1) Rotate ONLY the weapon holder toward mouse
        if (weaponHolder != null)
        {
            float angle = Mathf.Atan2(toMouse.y, toMouse.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRot = Quaternion.Euler(0f, 0f, angle);

            weaponHolder.rotation = Quaternion.Slerp(
                weaponHolder.rotation,
                targetRot,
                holderRotationSpeed * Time.deltaTime
            );

        }
        
        PlayerWeaponController pwc = GetComponent<PlayerWeaponController>();
        if (pwc != null && pwc.equippedWeapon != null && pwc.equippedWeapon.VisualRoot != null)
        {
            Transform visualRoot = pwc.equippedWeapon.VisualRoot;

            if (toMouse.x < 0f)
                visualRoot.localScale = new Vector3(1f, -1f, 1f);
            else
                visualRoot.localScale = new Vector3(1f, 1f, 1f);
        }
        
        if (playerVisual != null)
        {
            SetSpriteFromDirection(toMouse);
        }
        /*
        // Flip only the sprite visuals (NOT firepoint)
        var flip = weaponHolder.GetComponentInChildren<WeaponVisualFlip>();
        if (flip != null)
            flip.ApplyFlipFromAimDirection(aimDir);

    }
    var fp = GetComponent<PlayerWeaponController>()?.CurrentFirePoint;
    if (fp != null)
    {
        Debug.DrawRay(fp.position, fp.up * 2f, Color.cyan, 0f);   // fire direction
        Debug.DrawRay(fp.position, aimDir * 2f, Color.magenta, 0f); // mouse direction
    }
    // 2) Swap player sprite based on mouse direction (4-way)

        SetSpriteFromDirection(toMouse);
    */
    }

    private void UpdateWalkingSurface()
    {
        Vector2 playerFXtf = playerFX.transform.position;
        Collider2D hit = Physics2D.OverlapCircle(playerFXtf, 0.2f, roadLayer);
        DrawDebugCircle(playerFXtf, 0.2f, hit ? Color.red : Color.green, 0f);
//        Debug.Log("Road hit: " + (hit ? hit.name : "none"));

        SetWalkingColor(hit ? walkingColorRoad : walkingColorGrass);
    }

    private void SetWalkingColor(Color color)
    {
        var main = walkingFX.main;
        main.startColor = color;
    }

    private void SetSpriteFromDirection(Vector2 dir)
    {
        // angle where: right=0, up=90, left=180/-180, down=-90
        float a = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // 4-way thresholds at 45 degrees
        if (a >= -45f && a < 45f)              playerVisual.transform.localScale = new Vector3(1f, 1f, 1f);//gameObject.transform.localScale.Set(1f, 1f, 1f);
        else if (a >= 45f && a < 135f)         playerVisual.transform.localScale = new Vector3(1f, 1f, 1f);//gameObject.transform.localScale.Set(1f, 1f, 1f);
        else if (a >= -135f && a < -45f)       playerVisual.transform.localScale = new Vector3(-1f, 1f, 1f);//gameObject.transform.localScale.Set(-1f, 1f, 1f);
        else                                   playerVisual.transform.localScale = new Vector3(-1f, 1f, 1f);//gameObject.transform.localScale.Set(-1f, 1f, 1f);
    }
    
    public void SetPlayerGrounded(bool value)
    {
        isPlayerGrounded = value;
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


