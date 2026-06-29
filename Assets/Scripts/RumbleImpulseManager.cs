using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class RumbleImpulseManager : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource lowRumble;
    [SerializeField] private CinemachineImpulseSource rumble;
    [SerializeField] private CinemachineImpulseSource buildup;
    [SerializeField] private CinemachineImpulseSource breakImpulse;
    
    [SerializeField] private float lowRumbleInterval = 0.15f;
    [SerializeField] private float lowRumbleDuration = 1f;
    
    [SerializeField] private float rumbleInterval = 0.15f;
    [SerializeField] private float rumbleDuration = 1f;
    [SerializeField] private float instenciationWorldPadding = 1f;
    [SerializeField] private GameObject gameOverCanvas;
    
    [Header("Eviornment")]
    [SerializeField] private GameObject orbsPS;
    [SerializeField] private GameObject rayPS;
    [SerializeField] private GameObject rayGO;
    [SerializeField] private GameObject windGO;
    [SerializeField] private float rayDissapationDur = 1f;
    
    private List<ParticleSystem> particlesOrbs;
    private List<ParticleSystem> particlesRays;
    private List<SpriteRenderer> sprites;
    private List<SpriteRenderer> SpritesWind;
    private static readonly int Alpha = Shader.PropertyToID("_Alpha");
    private List<MaterialPropertyBlock> windMPBs;
    
    [Header("Whirlpool")]
    [SerializeField] private GameObject whirlpoolPrfab;
    [SerializeField] private GameObject hUDCanvas;
    [SerializeField] private GameObject tilemapGrid;
    [SerializeField] private UIRenderTextureProxySpawner uiTexProxySpawner;

    //private UICollapseController uICollapseController;
    private TilemapProxySpawner proxySpawner;
    private Camera cam;

    private WhirlpoolSequenceCoordinator currentWhirlpoolCoordinator;

    private void Start()
    {
        cam = Camera.main;
        //uICollapseController = hUDCanvas.GetComponent<UICollapseController>();
        proxySpawner = tilemapGrid.GetComponent<TilemapProxySpawner>();
        particlesOrbs= new List<ParticleSystem>();
        particlesRays = new List<ParticleSystem>();
        sprites = new List<SpriteRenderer>();
        
        SpritesWind = new List<SpriteRenderer>();
        
        foreach (SpriteRenderer renderers in rayGO.GetComponentsInChildren<SpriteRenderer>())
        {
            sprites.Add(renderers);
        }
        foreach (SpriteRenderer renderers in windGO.GetComponentsInChildren<SpriteRenderer>())
        {
            SpritesWind.Add(renderers);
        }
       /*foreach (SpriteRenderer renderer in sprites)
        {
            Debug.Log(renderer.name);
        }*/
        foreach (ParticleSystem system in rayPS.GetComponentsInChildren<ParticleSystem>())
        {
            particlesRays.Add(system);
        }
        foreach (ParticleSystem system in orbsPS.GetComponentsInChildren<ParticleSystem>())
        {
            particlesOrbs.Add(system);
        }
        foreach (ParticleSystem system in particlesOrbs)
        {
            Debug.Log("orbs list names: " + system.name);
        }
    }

    public void PlaySequence()
    {
        StartCoroutine(ImpulseSequence());
        Debug.LogWarning("WhirlpoolAnimManager.PlaySequence called on frame " + Time.frameCount);
    }

    private IEnumerator ImpulseSequence()
    { 
        float timer = 0f;
        StartCoroutine(LerpOrbParticleSizeToZero());
        foreach (ParticleSystem particle in particlesRays)
        {
            particle.emissionRate = 0f;
        }
        
        foreach (ParticleSystem particle in particlesOrbs)
        {
            particle.emissionRate = 0f;
        }
        
        
        
        while (timer < rumbleDuration)
        {
            rumble.GenerateImpulse(Random.insideUnitSphere * 0.2f + Vector3.left);
            yield return new WaitForSeconds(rumbleInterval);
            timer += rumbleInterval;
        }
        
        buildup.GenerateImpulse(Vector3.left);

        yield return new WaitForSeconds(0.35f);

        breakImpulse.GenerateImpulse(Vector3.down);

        yield return new WaitForSeconds(0.2f);
        
        GameObject whirlpoolInstance = Instantiate(whirlpoolPrfab, GetRandomWorldPosition(), Quaternion.identity);
        
        currentWhirlpoolCoordinator = whirlpoolInstance.GetComponent<WhirlpoolSequenceCoordinator>();
           Debug.LogWarning(
                    "Instantiated whirlpool: " + whirlpoolInstance.name +
                    " instanceID: " + currentWhirlpoolCoordinator.GetInstanceID() +
                    " scene time: " + Time.time
                );
           
        if (currentWhirlpoolCoordinator != null)
        {
            currentWhirlpoolCoordinator.AcquireGameOverGO(gameOverCanvas);
            currentWhirlpoolCoordinator.onCollapsePhaseReached.RemoveListener(OnCollapsePhaseReached);
            currentWhirlpoolCoordinator.onCollapsePhaseReached.AddListener(OnCollapsePhaseReached);
            currentWhirlpoolCoordinator.BeginSequence();
        }
        else
        {
            Debug.LogWarning("WhirlpoolSequenceCoordinator missing on whirlpool prefab root.");
        }
    

        StartCoroutine(LowRumbleLoop());
        
    }

    private IEnumerator LowRumbleLoop()
    {
        float timer = 0f;
        
        while (timer < lowRumbleDuration)
        {
            lowRumble.GenerateImpulse(Random.insideUnitSphere * 0.1f);
            yield return new WaitForSeconds(lowRumbleInterval);
            timer += lowRumbleInterval;
        }
    }

    private void OnCollapsePhaseReached()
    {
        //uiProxySpawner.SpawnUIProxies();
        //uICollapseController.Begin();

        StartCoroutine(LerpRayAlphaToZero());
        
        proxySpawner.SpawnProxies();
        
        uiTexProxySpawner.SpawnUITextureTiles();

        if (currentWhirlpoolCoordinator != null)
            currentWhirlpoolCoordinator.onCollapsePhaseReached.RemoveListener(OnCollapsePhaseReached);

        Destroy(gameObject);
    }

    private IEnumerator LerpRayAlphaToZero()
    {
        foreach (SpriteRenderer renderer in sprites)
        {
            float t = rayDissapationDur;
            float targetAlpha = 0f;
            Color targetColor = new Color(renderer.color.r, renderer.color.b, renderer.color.g, targetAlpha);
            renderer.color = Color.Lerp(renderer.color, targetColor, t);
        }

        MaterialPropertyBlock[] mpbs = new MaterialPropertyBlock[SpritesWind.Count];
        float[] startAlphas = new float[SpritesWind.Count];
        float windAlphaTarget = 0f;
        
        for (int i = 0; i < SpritesWind.Count; i++)
        {
            mpbs[i] = new MaterialPropertyBlock();

            SpritesWind[i].GetPropertyBlock(mpbs[i]);
            startAlphas[i] = mpbs[i].GetFloat(Alpha);
        }

        float elapsedTime = 0f;

        while (elapsedTime < rayDissapationDur)
        {
            elapsedTime += Time.deltaTime;
           
            
            float t = rayDissapationDur <= 0f ? 1f : Mathf.Clamp01(elapsedTime / rayDissapationDur);

            for (int i = 0; i < SpritesWind.Count; i++)
            {
                float value = Mathf.Lerp(startAlphas[i], windAlphaTarget, t);

                mpbs[i].SetFloat(Alpha, value);
                SpritesWind[i].SetPropertyBlock(mpbs[i]);
            }

            yield return null;
        }

        for (int i = 0; i < SpritesWind.Count; i++)
        {
            mpbs[i].SetFloat(Alpha, windAlphaTarget);
            SpritesWind[i].SetPropertyBlock(mpbs[i]);
        }

        yield return null;
    }
    
    private IEnumerator LerpOrbParticleSizeToZero()
    {
        Vector3[] startScales = new Vector3[particlesOrbs.Count];

        for (int i = 0; i < particlesOrbs.Count; i++)
        {
            if (particlesOrbs[i] == null) continue;

            startScales[i] = particlesOrbs[i].transform.localScale;
        }

        float t = 0f;

        while (t < rayDissapationDur)
        {
            t += Time.deltaTime;

            float normalizedTime = Mathf.Clamp01(t / rayDissapationDur);

            for (int i = 0; i < particlesOrbs.Count; i++)
            {
                if (particlesOrbs[i] == null) continue;

                Transform orbTransform = particlesOrbs[i].transform;

                orbTransform.localScale = Vector3.Lerp(
                    startScales[i],
                    Vector3.zero,
                    normalizedTime
                );
            }

            yield return null;
        }

        for (int i = 0; i < particlesOrbs.Count; i++)
        {
            if (particlesOrbs[i] == null) continue;

            particlesOrbs[i].transform.localScale = Vector3.zero;

            Debug.LogWarning(
                "Orb scaled to zero: " +
                particlesOrbs[i].gameObject.name +
                " | scale: " +
                particlesOrbs[i].transform.localScale
            );
        }
    }

    public Vector3 GetRandomWorldPosition()
    {
        float randomX = Random.Range(instenciationWorldPadding, 1f - instenciationWorldPadding);
        float randomY = Random.Range(instenciationWorldPadding, 1f - instenciationWorldPadding);

        Vector3 viewportPoint = new Vector3(randomX, randomY, 0f);
        Vector3 worldPos = cam.ViewportToWorldPoint(viewportPoint);
        worldPos.z = 0f;

        return worldPos;
    }
    
}