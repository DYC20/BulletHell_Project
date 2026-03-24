using System.Collections.Generic;
using UnityEngine;

public class UICollapseController : MonoBehaviour
{
    [Header("Collection")]
    [SerializeField] private RectTransform collectionRoot;
    [SerializeField] private LayerMask affectedLayers = ~0;
    [SerializeField] private bool includeInactive = false;
    [SerializeField] private bool beginOnStart = false;

    [Header("Motion")]
    [SerializeField] private float moveSpeed = 600f;
    [SerializeField] private float swirlStrength = 120f;
    [SerializeField] private bool clockwise = true;
    [SerializeField] private float consumeDistance = 8f;
    [SerializeField] private float referenceDistance = 800f;

    [Header("Visuals")]
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

    private struct UIData
    {
        public RectTransform rect;
        public Vector3 startScale;
        public CanvasGroup canvasGroup;
        public float startAlpha;
        public bool consumed;
    }

    private readonly List<UIData> _targets = new();

    private bool _running;
    private int _activeCount;

    private void Start()
    {
        if (beginOnStart)
            Begin();
    }

    public void Begin()
    {
        CollectTargets();
        _running = _activeCount > 0;
    }

    public void StopEffect()
    {
        _running = false;
    }

    private void Update()
    {
        if (!_running)
            return;

        Vector2 center = Vector2.zero;

        for (int i = 0; i < _targets.Count; i++)
        {
            UIData data = _targets[i];

            if (data.consumed || data.rect == null)
                continue;

            Vector2 pos = data.rect.anchoredPosition;
            Vector2 toCenter = center - pos;
            float dist = toCenter.magnitude;

            if (dist <= consumeDistance)
            {
                Consume(i);
                continue;
            }

            Vector2 dir = dist > 0.0001f ? toCenter / dist : Vector2.zero;

            Vector2 tangent = clockwise
                ? new Vector2(dir.y, -dir.x)
                : new Vector2(-dir.y, dir.x);

            float normalizedDistance = Mathf.Clamp01(dist / Mathf.Max(1f, referenceDistance));

            float pullFactor = 1f - normalizedDistance;
            float swirlFactor = normalizedDistance * normalizedDistance;

            float pull = Mathf.Max(0.25f, moveSpeed * pullFactor);
            float swirl = swirlStrength * swirlFactor;

            swirl = Mathf.Min(swirl, pull * 0.8f);

            Vector2 motion = (dir * pull + tangent * swirl) * Time.deltaTime;
            data.rect.anchoredPosition += motion;

            float progress = 1f - normalizedDistance;

            data.rect.localScale = data.startScale * scaleCurve.Evaluate(progress);
            data.canvasGroup.alpha = data.startAlpha * fadeCurve.Evaluate(progress);

            _targets[i] = data;
        }

        if (_activeCount <= 0)
            _running = false;
    }

    private void CollectTargets()
    {
        _targets.Clear();
        _activeCount = 0;

        if (collectionRoot == null)
        {
            Debug.LogWarning("UICollapseController: collectionRoot is not assigned.", this);
            return;
        }

        for (int i = 0; i < collectionRoot.childCount; i++)
        {
            RectTransform child = collectionRoot.GetChild(i) as RectTransform;
            if (child == null)
                continue;

            if (!includeInactive && !child.gameObject.activeInHierarchy)
                continue;

            if (((1 << child.gameObject.layer) & affectedLayers.value) == 0)
                continue;

            CanvasGroup canvasGroup = child.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = child.gameObject.AddComponent<CanvasGroup>();

            UIData data = new UIData
            {
                rect = child,
                startScale = child.localScale,
                canvasGroup = canvasGroup,
                startAlpha = canvasGroup.alpha,
                consumed = false
            };

            _targets.Add(data);
            _activeCount++;
        }
    }

    private void Consume(int index)
    {
        UIData data = _targets[index];
        data.consumed = true;
        _targets[index] = data;
        _activeCount--;

        if (data.rect != null)
            data.rect.gameObject.SetActive(false);
    }
}