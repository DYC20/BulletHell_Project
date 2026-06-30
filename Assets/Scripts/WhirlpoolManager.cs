using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class WhirlpoolManager : MonoBehaviour
{
    public static WhirlpoolManager Instance { get; private set; }

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
    [SerializeField] private float deathRadius = 0.2f;
    
   // [Header("Sounds")]
    //[SerializeField] private List<AudioSource> collapseSuctionSources = new List<AudioSource>();
    //[SerializeField] private float collapseSuctionDelay = 0.25f;

    private bool collapseSuctionScheduled;
    private Coroutine collapseSuctionRoutine;

    [SerializeField] private AudioSource disappearOneShotSource;
    [SerializeField] private List<AudioClip> disappearSuctionClips = new List<AudioClip>();
    [SerializeField] private float disappearSuctionVolume = 1f;

    private bool collapseSuctionPlayed;
    
    [SerializeField] private WhirlpoolAnimManager _WAM;

    private GameObject gameOverCanvas;
    private RumbleImpulseManager rumbleImpulseManager;
    [SerializeField] private PlayableAsset finnTimeline;
    
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
        public bool isProxyTile;
    }

    private void Awake()
    {
        Instance = this;
    }

    public void PullSeqUence()
    {
        Begin();
    }

    public void Begin()
    {
        Debug.LogWarning("Begin Pull Sequence");
        if (vortexCenter == null)
            vortexCenter = transform;

        DisablePlayerInput();
        
        collapseSuctionPlayed = false;
        _targets.Clear();
        _uniqueTargets.Clear();
        _elapsed = 0f;
        _running = true;
        _activeTargets = 0;
        
        collapseSuctionScheduled = false;

        if (collapseSuctionRoutine != null)
        {
            StopCoroutine(collapseSuctionRoutine);
            collapseSuctionRoutine = null;
        }

        CollectSceneTargets();
    }

    private void Update()
    {
        if (!_running)
            return;

        _elapsed += Time.deltaTime;

        UpdateTargets();

        if (_activeTargets <= 0)
            _running = false;
        else
        {
            _running = true;
        }
    }

    public void RegisterExternalTarget(Transform t)
    {
        if (t == null || vortexCenter == null)
            return;

        if (_uniqueTargets.Contains(t))
            return;

        float distance = Vector2.Distance(vortexCenter.position, t.position);
        if (distance > pullAOE)
            return;

        AddTarget(t, distance, true);
        _running = true;
    }

    private void CollectSceneTargets()
    {
        Debug.LogWarning("Collect Scene Targets");
        Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);

        foreach (var r in renderers)
        {
            Transform root = r.transform.root;

            if (_uniqueTargets.Contains(root))
                continue;

            if (((1 << root.gameObject.layer) & affectedLayers.value) == 0)
                continue;

            float distance = Vector2.Distance(vortexCenter.position, root.position);
            if (distance > pullAOE)
                continue;

            AddTarget(root, distance, false);
        }
    }

    private void AddTarget(Transform t, float distance,  bool isProxyTile)
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
            consumed = false,
            isProxyTile = isProxyTile
        };

        _targets.Add(data);
        _activeTargets++;
    }

    private void UpdateTargets()
    {
        Debug.LogWarning("Update Targets");
        Vector2 center = vortexCenter.position;

        for (int i = 0; i < _targets.Count; i++)
        {
            var target = _targets[i];

            if (target.consumed || target.transform == null)
                continue;

            if (_elapsed < target.startDelay)
                continue;
            /*
            if (target.isProxyTile && !collapseSuctionScheduled)
            {
                collapseSuctionScheduled = true;
                collapseSuctionRoutine = StartCoroutine(PlayCollapseSuctionAfterDelay());
            }
*/
            Vector2 pos = target.transform.position;
            Vector2 toCenter = center - pos;
            float dist = toCenter.magnitude;

            if (dist <= deathRadius)
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
    /*
    private IEnumerator PlayCollapseSuctionAfterDelay()
    {
        yield return new WaitForSeconds(collapseSuctionDelay);

        PlayCollapseSuction();

        collapseSuctionRoutine = null;
    }
*/
    private void Consume(TargetData target)
    {
        Debug.LogWarning("Active Target:" + _activeTargets);
        target.consumed = true;
        _activeTargets--;
        
        if (target.isProxyTile)
        {
            PlayRandomDisappearSuction();
        }
        if (target.transform != null)
            target.transform.gameObject.SetActive(false);

        if (_activeTargets <= 0 && _targets.Count > 1 )
        {
            Debug.LogWarning("Active Targets <= 0");
            gameOverCanvas.gameObject.SetActive(true);
            PlayableDirector _gameOverDirector = gameOverCanvas.GetComponent<PlayableDirector>();
            //_gameOverDirector.playableAsset = finnTimeline;
            Debug.LogWarning("Game Over Playable assigned");
            _gameOverDirector.Play(finnTimeline);
        }
    }

    private void DisablePlayerInput()
    {
        var input = FindFirstObjectByType<PlayerInput>();
        if (input == null)
            return;

        input.enabled = false;

        var rb = input.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }
    /*
    private void PlayCollapseSuction()
    {
        foreach (AudioSource source in collapseSuctionSources)
        {
            if (source == null || source.clip == null)
                continue;

            source.loop = false;
            source.time = 0f;
            source.Play();
        }
    }
    */
    private void PlayRandomDisappearSuction()
    {
        if (disappearOneShotSource == null)
            return;

        if (disappearSuctionClips == null || disappearSuctionClips.Count == 0)
            return;

        AudioClip clip = disappearSuctionClips[Random.Range(0, disappearSuctionClips.Count)];

        if (clip == null)
            return;

        disappearOneShotSource.loop = false;
        disappearOneShotSource.PlayOneShot(clip, disappearSuctionVolume);
    }
    public void AcquireGameOverGO(GameObject gameOverGO)
    {
        gameOverCanvas = gameOverGO;
    }
    public void AcquireRumbleImpulseManager(RumbleImpulseManager manager)
    {
        rumbleImpulseManager = manager;
    }
}