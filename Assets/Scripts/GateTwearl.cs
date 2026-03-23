using UnityEngine;

public class GateTwearl : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float duration = 2f;
    [SerializeField] private bool playOnEnable = true;

    [Header("Detection")]
    [SerializeField] private float pullAOE = 5f;
    [SerializeField] private LayerMask affectedLayers = ~0;

    [Header("Movement")]
    [SerializeField] private float pullStrength = 6f;
    [SerializeField] private float swirlStrength = 4f;
    [SerializeField] private bool clockwise = true;

    [Header("Curves Over Lifetime")]
    [Tooltip("Controls inward pull over normalized time (0 -> 1).")]
    [SerializeField] private AnimationCurve pullOverTime = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Tooltip("Controls sideways swirl over normalized time (0 -> 1).")]
    [SerializeField] private AnimationCurve swirlOverTime = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Header("Distance Influence")]
    [Tooltip("Controls how strong the pull is depending on distance from center. X = normalized distance, Y = multiplier.")]
    [SerializeField] private AnimationCurve pullByDistance =
        new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.25f, 0.35f),
            new Keyframe(1f, 1f)
        );

    [Tooltip("Controls how strong the swirl is depending on distance from center. X = normalized distance, Y = multiplier.")]
    [SerializeField] private AnimationCurve swirlByDistance =
        new AnimationCurve(
            new Keyframe(0f, 0.15f),
            new Keyframe(0.6f, 1f),
            new Keyframe(1f, 0.6f)
        );

    [Header("Center Zone")]
    [Tooltip("When objects get very close, stop the swirl so they collapse nicely into the center.")]
    [SerializeField] private float innerDeadZone = 0.15f;

    [Header("Target Handling")]
    [Tooltip("If true, Rigidbody2D targets are moved with MovePosition. Otherwise transform.position is used.")]
    [SerializeField] private bool useRigidbodyIfAvailable = true;

    private float _elapsed;
    private bool _isActive;

    private void OnEnable()
    {
        if (playOnEnable)
            Begin();
    }

    private void Update()
    {
        if (!_isActive)
            return;

        _elapsed += Time.deltaTime;

        float normalizedTime = duration <= 0f ? 1f : Mathf.Clamp01(_elapsed / duration);

        ApplyWhirlpool(normalizedTime);

        if (_elapsed >= duration)
            _isActive = false;
    }

    public void Begin()
    {
        _elapsed = 0f;
        _isActive = true;
    }

    public void Stop()
    {
        _isActive = false;
    }

    private void ApplyWhirlpool(float normalizedTime)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, pullAOE, affectedLayers);

        float timePullMultiplier = pullOverTime.Evaluate(normalizedTime);
        float timeSwirlMultiplier = swirlOverTime.Evaluate(normalizedTime);

        Vector2 center = transform.position;

        foreach (Collider2D col in colliders)
        {
            if (col == null)
                continue;

            if (col.transform == transform)
                continue;

            Vector2 currentPos = col.attachedRigidbody != null
                ? col.attachedRigidbody.position
                : (Vector2)col.transform.position;

            Vector2 toCenter = center - currentPos;
            float distance = toCenter.magnitude;

            if (distance <= 0.0001f)
                continue;

            Vector2 inwardDir = toCenter / distance;

            float normalizedDistance = Mathf.Clamp01(distance / pullAOE);

            float distancePullMultiplier = pullByDistance.Evaluate(normalizedDistance);
            float distanceSwirlMultiplier = swirlByDistance.Evaluate(normalizedDistance);

            float currentPullStrength = pullStrength * timePullMultiplier * distancePullMultiplier;
            float currentSwirlStrength = swirlStrength * timeSwirlMultiplier * distanceSwirlMultiplier;

            // Perpendicular vector creates the curved / spiral path.
            Vector2 tangentDir = clockwise
                ? new Vector2(inwardDir.y, -inwardDir.x)
                : new Vector2(-inwardDir.y, inwardDir.x);

            // Reduce swirl very close to the center so targets collapse inward cleanly.
            if (distance <= innerDeadZone)
                currentSwirlStrength = 0f;

            Vector2 velocity =
                inwardDir * currentPullStrength +
                tangentDir * currentSwirlStrength;

            Vector2 nextPos = currentPos + velocity * Time.deltaTime;

            if (useRigidbodyIfAvailable && col.attachedRigidbody != null && !col.attachedRigidbody.isKinematic)
            {
                col.attachedRigidbody.MovePosition(nextPos);
            }
            else
            {
                col.transform.position = nextPos;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pullAOE);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, innerDeadZone);
    }
}