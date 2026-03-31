using UnityEngine;
using UnityEngine.Events;

public class EndOfPSTrigger : MonoBehaviour
{
    private ParticleSystem particleSystem;
    private bool hasTriggered;

    public UnityEvent deathEvent;

    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        if (hasTriggered || particleSystem == null)
            return;

        if (!particleSystem.IsAlive(false))
            DeathTrigger();
    }

    public void DeathTrigger()
    {
        if (hasTriggered)
            return;

        hasTriggered = true;
        deathEvent.Invoke();
    }
}