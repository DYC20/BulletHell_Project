using UnityEngine;

public class PSLifetimeByMovement : MonoBehaviour
{
    [SerializeField] private ParticleSystem ps;

    [Header("Lifetime")]
    [SerializeField] private float idleLifetime = 1.2f;
    [SerializeField] private float movingLifetime = 0.25f;

    [Header("Emmision")] 
    [SerializeField] private float idleEmmision;
    [SerializeField] private float movingEmmision;
    
    [Header("Movement")]
    [SerializeField] private float speedForMinLifetime = 5f;

    private Vector3 lastPosition;

    private void Awake()
    {
        if (ps == null)
            ps = GetComponent<ParticleSystem>();

        lastPosition = transform.position;
    }

    private void LateUpdate()
    {
        float speed = (transform.position - lastPosition).magnitude / Time.deltaTime;
        lastPosition = transform.position;

        float t = Mathf.InverseLerp(0f, speedForMinLifetime, speed);
        float lifetime = Mathf.Lerp(idleLifetime, movingLifetime, t);
        float emmisionRate = Mathf.Lerp(idleEmmision, movingEmmision, t);

        var main = ps.main;
        main.startLifetime = lifetime;
        
        var emmision = ps.emission;
        emmision.rateOverTime = emmisionRate;
    }
}
