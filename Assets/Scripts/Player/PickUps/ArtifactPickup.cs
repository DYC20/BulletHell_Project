using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using Random = UnityEngine.Random;

public class ArtifactPickup : MonoBehaviour, IPickup
{
   /* [SerializeField] private GameObject whirlpoolPrfab;
    [SerializeField] private GameObject hUDCanvas;
    [SerializeField] private GameObject tilemapGrid;
    private UICollapseController uICollapseController;
    private TilemapProxySpawner proxySpawner;
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
        uICollapseController = hUDCanvas.GetComponent<UICollapseController>();
        proxySpawner = tilemapGrid.GetComponent<TilemapProxySpawner>();
    }*/
   [SerializeField] private PlayableDirector director;
   [SerializeField] private Material lowHealthShader;
   [SerializeField] private float _LHSDuration;

   private RumbleImpulseManager manager; 
   private bool pickedUp = false;
   private float transitionValue;
   
   private static readonly int controllEffect = Shader.PropertyToID("_ControlEffect");
    public bool CanPickup(GameObject picker)
    {
        Debug.LogWarning("CanPickup artifact");
        if (pickedUp)
            return false;
        return true;
    }

    public void Pickup(GameObject picker)
    {
        Debug.LogWarning("Pickup artifact");
        manager = GetComponent<RumbleImpulseManager>();
        if (manager == null) Debug.Log("Artifact Pickup Failed");
        if (manager != null)
        {
            Debug.Log("manager GO:" + manager.GetType().Name);
        }
        
        StartCoroutine(DisableLowHealthEffect());
        if (pickedUp)
            return;
        pickedUp = true;
        director.Play();

    }

    public void PlayManagerSequence()
    {
        manager.PlaySequence();
        //Instantiate(whirlpoolPrfab, GetRandomWorldPosition(), Quaternion.identity);
        //uICollapseController.Begin();
        //proxySpawner.SpawnProxies();
        Renderer renderer = GetComponent<SpriteRenderer>();
        renderer.enabled = false;
    }
    
    private IEnumerator DisableLowHealthEffect()
    {
        float timer = 0f;
        
        float startValue = lowHealthShader.GetFloat("controllEffect");
        
        while (timer < 1f)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / _LHSDuration);
            transitionValue = Mathf.Lerp(startValue, 0f, t);
            lowHealthShader.SetFloat(controllEffect, transitionValue);
            yield return null;
        }
        lowHealthShader.SetFloat(controllEffect, 0f);
    }
/*
    public Vector3 GetRandomWorldPosition(float padding = 0.1f)
    {
        float randomX = Random.Range(padding, 1f - padding);
        float randomY = Random.Range(padding, 1f - padding);

        Vector3 viewportPoint = new Vector3(randomX, randomY, 0f);

        Vector3 worldPos = cam.ViewportToWorldPoint(viewportPoint);

        // Important for 2D: fix Z
        worldPos.z = 0f;

        return worldPos;
    }*/
}
