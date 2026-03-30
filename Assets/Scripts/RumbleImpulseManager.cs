using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class RumbleImpulseManager : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource rumble;
    [SerializeField] private CinemachineImpulseSource buildup;
    [SerializeField] private CinemachineImpulseSource breakImpulse;

    [SerializeField] private float rumbleInterval = 0.15f;
    [SerializeField] private float rumbleDuration = 1f;

    [Header("Whirlpool")]
    [SerializeField] private GameObject whirlpoolPrfab;
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
    }
    public void PlaySequence()
    {
        StartCoroutine(ImpulseSequence());
    }

    private IEnumerator ImpulseSequence()
    {
        // 1. RUMBLE LOOP
        float timer = 0f;

        while (timer < rumbleDuration)
        {
            rumble.GenerateImpulse(Random.insideUnitSphere * 0.2f + Vector3.left);
            yield return new WaitForSeconds(rumbleInterval);
            timer += rumbleInterval;
        }

        // 2. BUILD-UP
        buildup.GenerateImpulse(Vector3.left);

        yield return new WaitForSeconds(0.35f);

        // 3. BREAK
        breakImpulse.GenerateImpulse(Vector3.down);
        
        yield return new WaitForSeconds(0.2f);
        
        Instantiate(whirlpoolPrfab, GetRandomWorldPosition(), Quaternion.identity);
        
        yield return new WaitForSeconds(8f);
        
        uICollapseController.Begin();
        proxySpawner.SpawnProxies();

        Destroy(gameObject);
    }
    
    public Vector3 GetRandomWorldPosition(float padding = 0.1f)
    {
        float randomX = Random.Range(padding, 1f - padding);
        float randomY = Random.Range(padding, 1f - padding);

        Vector3 viewportPoint = new Vector3(randomX, randomY, 0f);

        Vector3 worldPos = cam.ViewportToWorldPoint(viewportPoint);

        // Important for 2D: fix Z
        worldPos.z = 0f;

        return worldPos;
    }
    
}
