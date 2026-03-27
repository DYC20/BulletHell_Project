using System;
using UnityEngine;
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

    public bool CanPickup(GameObject picker)
    {
        return true;
    }

    public void Pickup(GameObject picker)
    {
        RumbleImpulseManager manager = GetComponent<RumbleImpulseManager>();
        if (manager == null) Debug.Log("Rtifact Pickup Failed");
        if (manager != null)
        {
            Debug.Log("manager GO:" + manager.GetType().Name);
        }
        manager.PlaySequence();
        //Instantiate(whirlpoolPrfab, GetRandomWorldPosition(), Quaternion.identity);
        //uICollapseController.Begin();
        //proxySpawner.SpawnProxies();
        Renderer renderer = GetComponent<SpriteRenderer>();
        renderer.enabled = false;
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
