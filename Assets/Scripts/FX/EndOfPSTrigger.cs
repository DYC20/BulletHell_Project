using UnityEngine;
using UnityEngine.Events;

public class EndOfPSTrigger : MonoBehaviour
{
    private ParticleSystem particleSystem;
    public UnityEvent deathEvent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!particleSystem.isEmitting)
            DeathTrigger();
    }

    public void DeathTrigger()
    {
        deathEvent.Invoke();
    }
}
