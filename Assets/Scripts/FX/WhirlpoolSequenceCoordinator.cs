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

        animationStarted = false;
        pullStarted = false;
        collapseTriggered = false;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // 1. Start animation
        if (!animationStarted && timer >= animationStartDelay)
        {
            whirlpoolAnimManager?.PlaySequence();
            animationStarted = true;
        }

        // 2. Start pull
        if (!pullStarted && timer >= pullSequenceDelay)
        {
            whirlpoolManager?.PullSeqUence();
            pullStarted = true;
        }

        // 3. Start collapse
        if (!collapseTriggered && timer >= collapsePhaseDelay)
        {
            onCollapsePhaseReached.Invoke();
            collapseTriggered = true;
        }
    }
}