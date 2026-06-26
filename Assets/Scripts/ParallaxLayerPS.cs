using System;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(ParticleSystem))]
public class ParallaxLayerPS : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;

    [Range(0f, 2f)]
    [SerializeField] private float parallaxFactor = 0.5f;

    [SerializeField] private float referenceSize;
    [SerializeField] private bool onlyXAxis = true;
    
    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;
    
    private Vector3 lastCameraPosition;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[ps.main.maxParticles];
    }

    private void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        lastCameraPosition = cameraTransform.position;
    }

    private void LateUpdate()
    {
        int aliveParticles = ps.GetParticles(particles);
        Vector3 cameraDelta = cameraTransform.position - lastCameraPosition;
        
        for (int i = 0; i < aliveParticles; i++)
        {
            ParticleSystem.Particle p = particles[i];
            float scale = p.startSize / referenceSize;

            if (onlyXAxis)
            {
                p.position += new Vector3(
                    cameraDelta.x * parallaxFactor,
                    0f,
                    0f
                ) * scale;
            }
            else
            {
                p.position += new Vector3(
                    cameraDelta.x * parallaxFactor,
                    cameraDelta.y * parallaxFactor,
                    0f
                ) * scale;
            }

            particles[i] = p;
        }
        ps.SetParticles(particles, aliveParticles);
        lastCameraPosition = cameraTransform.position;
    }


}