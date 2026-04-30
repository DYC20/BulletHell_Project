using UnityEngine;
using UnityEngine.Events;

public class WhirlpoolSequenceCoordinator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WhirlpoolAnimManager whirlpoolAnimManager;
    [SerializeField] private WhirlpoolManager whirlpoolManager;

    [Header("Timing (from spawn)")]
    [SerializeField] private float animationStartDelay = 0f;
    [SerializeField] private float pullSequenceDelay = 0.2f;
    [SerializeField] private float collapsePhaseDelay = 0.6f;

    public UnityEvent onCollapsePhaseReached;

    private float timer;

    private bool sequenceStarted;
    private bool animationStarted;
    private bool pullStarted;
    private bool collapseTriggered;

    private void Awake()
    {
        if (whirlpoolAnimManager == null)
            whirlpoolAnimManager = GetComponentInChildren<WhirlpoolAnimManager>(true);

        if (whirlpoolManager == null)
            whirlpoolManager = GetComponentInChildren<WhirlpoolManager>(true);
    }

    public void BeginSequence()
    {
        timer = 0f;

        sequenceStarted = true;
        animationStarted = false;
        pullStarted = false;
        collapseTriggered = false;

        Debug.LogWarning(
            "BeginSequence on: " + gameObject.name +
            " instanceID: " + GetInstanceID() +
            " scene time: " + Time.time);
    }

    private void Update()
    {
        if (!sequenceStarted) return;
           
        
        timer += Time.deltaTime;

        // 1. Start animation
        if (!animationStarted && timer >= animationStartDelay)
        {
            whirlpoolAnimManager?.PlaySequence();
            animationStarted = true;
            Debug.LogWarning("Animation Started");
            Debug.LogWarning("animation Start Delay: " + animationStartDelay);
            Debug.LogWarning("Time at animation begin: " + timer);
            Debug.LogWarning(
                "Animation Started on: " + gameObject.name +
                " instanceID: " + GetInstanceID() +
                " timer: " + timer +
                " scene time: " + Time.time
            );
        }

        // 2. Start pull
        if (!pullStarted && timer >= pullSequenceDelay)
        {
            whirlpoolManager?.PullSeqUence();
            pullStarted = true;
            Debug.LogWarning("Pull Called");
            Debug.LogWarning("pullSequenceDelay: " + pullSequenceDelay);
            Debug.LogWarning("Time at pull begin: " + timer);
        }

        // 3. Start collapse
        if (!collapseTriggered && timer >= collapsePhaseDelay)
        {
            onCollapsePhaseReached.Invoke();
            collapseTriggered = true;
            Debug.LogWarning("Collapse Triggered");
            Debug.LogWarning("collapsePhaseDelay: " + collapsePhaseDelay);
            Debug.LogWarning("Time at collapse begin: " + timer);
        }
    }
}