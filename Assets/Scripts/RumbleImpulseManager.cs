using System.Collections;
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

    [Header("Whirlpool")]
    [SerializeField] private GameObject whirlpoolPrfab;
    [SerializeField] private GameObject hUDCanvas;
    [SerializeField] private GameObject tilemapGrid;

    private UICollapseController uICollapseController;
    private TilemapProxySpawner proxySpawner;
    private Camera cam;

    private WhirlpoolSequenceCoordinator currentWhirlpoolCoordinator;

    private void Start()
    {
        cam = Camera.main;
        uICollapseController = hUDCanvas.GetComponent<UICollapseController>();
        proxySpawner = tilemapGrid.GetComponent<TilemapProxySpawner>();
    }

    public void PlaySequence()
    {
        StartCoroutine(ImpulseSequence());
        Debug.LogWarning("WhirlpoolAnimManager.PlaySequence called on frame " + Time.frameCount);
    }

    private IEnumerator ImpulseSequence()
    { 
       
        
        float timer = 0f;

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
        
        Debug.LogWarning(
            "Instantiated whirlpool: " + whirlpoolInstance.name +
            " instanceID: " + currentWhirlpoolCoordinator.GetInstanceID() +
            " scene time: " + Time.time
        );
     
        currentWhirlpoolCoordinator = whirlpoolInstance.GetComponent<WhirlpoolSequenceCoordinator>();
        
        if (currentWhirlpoolCoordinator != null)
        {
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
        uICollapseController.Begin();
        proxySpawner.SpawnProxies();

        if (currentWhirlpoolCoordinator != null)
            currentWhirlpoolCoordinator.onCollapsePhaseReached.RemoveListener(OnCollapsePhaseReached);

        Destroy(gameObject);
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