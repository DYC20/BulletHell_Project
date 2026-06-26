using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class KillParticlesOutsideCollider2D : MonoBehaviour
{
    [SerializeField] private BoxCollider2D boundsCollider;

    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();

        var main = ps.main;
        particles = new ParticleSystem.Particle[main.maxParticles];
    }

    private void LateUpdate()
    {
        if (boundsCollider == null) return;

        int count = ps.GetParticles(particles);

        //bool localSpace = ps.main.simulationSpace == ParticleSystemSimulationSpace.Local;

        for (int i = 0; i < count; i++)
        {
            Vector3 particlePosition = particles[i].position;
/*
            if (localSpace)
                particlePosition = transform.TransformPoint(particlePosition);
*/
            if (!boundsCollider.OverlapPoint(particlePosition))
            {
                particles[i].remainingLifetime = 0f;
            }
        }

        ps.SetParticles(particles, count);
    }

    private void OnDrawGizmosSelected()
    {
        if (boundsCollider == null) return;

        Gizmos.matrix = boundsCollider.transform.localToWorldMatrix;
        Gizmos.DrawWireCube(boundsCollider.offset, boundsCollider.size);
    }
}