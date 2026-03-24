using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WhirlpoolManager : MonoBehaviour
{
    [Header("Center")]
    [SerializeField] private Transform vortexCenter;

    [Header("Radius")]
    [SerializeField] private float pullAOE = 10f;

    [Header("Layers")]
    [SerializeField] private LayerMask affectedLayers;

    [Header("Motion")]
    [SerializeField] private float pullStrength = 8f;
    [SerializeField] private float swirlStrength = 5f;
    [SerializeField] private bool clockwise = true;

    [Header("Delay")]
    [SerializeField] private bool useDistanceDelay = true;
    [SerializeField] private float maxDelay = 1f;
    [SerializeField] private bool invertDelay = true;
    [SerializeField] private AnimationCurve delayByDistance;
    [SerializeField] private float delayRandomness = 0.2f;

    [Header("Consume")]
    [SerializeField] private float consumeRadius = 0.2f;

    [SerializeField] private bool beginOnStart = false;

    private float _elapsed;
    private bool _running;

    private readonly List<TargetData> _targets = new();
    private readonly HashSet<Transform> _uniqueTargets = new();

    private int _activeTargets;

    private class TargetData
    {
        public Transform transform;
        public Vector3 startScale;
        public float startDelay;
        public float strengthMultiplier;
        public float spinSpeed;
        public bool consumed;
    }

    private void Start()
    {
        if (vortexCenter == null)
            vortexCenter = transform;

        if (beginOnStart)
            Begin();
    }

    public void Begin()
    {
        DisablePlayerInput();

        _targets.Clear();
        _uniqueTargets.Clear();
        _elapsed = 0f;
        _running = true;
        _activeTargets = 0;

        CollectSceneTargets();
    }

    private void Update()
    {
        if (!_running) return;

        _elapsed += Time.deltaTime;

        UpdateTargets();

        if (_activeTargets <= 0)
            _running = false;
    }

    public void RegisterExternalTarget(Transform t)
    {
        if (t == null) return;
        if (_uniqueTargets.Contains(t)) return;

        float distance = Vector2.Distance(vortexCenter.position, t.position);
        if (distance > pullAOE) return;

        AddTarget(t, distance);
    }

    private void CollectSceneTargets()
    {
        Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);

        foreach (var r in renderers)
        {
            Transform root = r.transform.root;

            if (_uniqueTargets.Contains(root)) continue;

            if (((1 << root.gameObject.layer) & affectedLayers.value) == 0)
                continue;

            float distance = Vector2.Distance(vortexCenter.position, root.position);
            if (distance > pullAOE) continue;

            AddTarget(root, distance);
        }
    }

    private void AddTarget(Transform t, float distance)
    {
        _uniqueTargets.Add(t);

        float normalizedDistance = Mathf.Clamp01(distance / pullAOE);

        if (invertDelay)
            normalizedDistance = 1f - normalizedDistance;

        float delayFactor = delayByDistance.Evaluate(normalizedDistance);
        float delay = useDistanceDelay ? delayFactor * maxDelay : 0f;

        delay += Random.Range(0f, delayRandomness);
        delay = Mathf.Max(0f, delay);

        TargetData data = new TargetData
        {
            transform = t,
            startScale = t.localScale,
            startDelay = delay,
            strengthMultiplier = 1f + Random.Range(-0.2f, 0.2f),
            spinSpeed = Random.Range(-360f, 360f),
            consumed = false
        };

        _targets.Add(data);
        _activeTargets++;
    }

    private void UpdateTargets()
    {
        Vector2 center = vortexCenter.position;

        for (int i = 0; i < _targets.Count; i++)
        {
            var target = _targets[i];

            if (target.consumed || target.transform == null)
                continue;

            if (_elapsed < target.startDelay)
                continue;

            Vector2 pos = target.transform.position;
            Vector2 toCenter = center - pos;
            float dist = toCenter.magnitude;

            if (dist <= consumeRadius)
            {
                Consume(target);
                continue;
            }

            Vector2 dir = toCenter / dist;

            Vector2 tangent = clockwise
                ? new Vector2(dir.y, -dir.x)
                : new Vector2(-dir.y, dir.x);

            float normalizedDistance = Mathf.Clamp01(dist / pullAOE);

            float pullFactor = 1f - normalizedDistance;
            float swirlFactor = Mathf.Pow(normalizedDistance, 2f);

            float pull = Mathf.Max(0.25f, pullStrength * pullFactor) * target.strengthMultiplier;
            float swirl = swirlStrength * swirlFactor * target.strengthMultiplier;

            swirl = Mathf.Min(swirl, pull * 0.8f);

            Vector2 motion = (dir * pull + tangent * swirl) * Time.deltaTime;

            target.transform.position += (Vector3)motion;
            target.transform.Rotate(0f, 0f, target.spinSpeed * Time.deltaTime);
            target.transform.localScale = target.startScale * normalizedDistance;
        }
    }

    private void Consume(TargetData target)
    {
        target.consumed = true;
        _activeTargets--;

        if (target.transform != null)
            target.transform.gameObject.SetActive(false);
    }

    private void DisablePlayerInput()
    {
        var input = FindFirstObjectByType<PlayerInput>();
        if (input == null) return;

        input.enabled = false;

        var rb = input.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }
}