using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightFllicker : MonoBehaviour
{
    [SerializeField] private Light2D light2D;
    [SerializeField] private float fllickerSpeed = 0.2f;
    [SerializeField] private float fllickerRadiusSpeed = 0.2f;
    [SerializeField] private float fllickerRadiusMax = 6f;
    [SerializeField] private float fllickerRadiusMin = 6f;
    [SerializeField] private float fllickerIntencityMax = 0.2f;
    [SerializeField] private float fllickerIntencityMin = 0.2f;
    float flickerTimer;
    float radiusTimer;
    float fromRadius;
    float toRadius;
    float fromIntensity;
    float toIntensity;

    void Start()
    {
        FindLight();
        PickNewFlickerTarget();
        PickNewRadiusTarget();
    }

    private void OnEnable()
    {
        FindLight();

        if (light2D == null)
            return;

        PickNewFlickerTarget();
        PickNewRadiusTarget();
    }
    
    void Update()
    {
        if (light2D == null)
            return;
        
        flickerTimer += Time.deltaTime * fllickerSpeed;
        radiusTimer += Time.deltaTime * fllickerRadiusSpeed;

        light2D.intensity = Mathf.Lerp(fromIntensity, toIntensity, flickerTimer);
        light2D.pointLightOuterRadius = Mathf.Lerp(fromRadius, toRadius, radiusTimer);

        if (flickerTimer >= 1f)
            PickNewFlickerTarget();
        if (radiusTimer >= 1F)
            PickNewRadiusTarget();
    }
    private void FindLight()
    {
        if (light2D == null)
            light2D = GetComponentInChildren<Light2D>(true);
    }

    void PickNewFlickerTarget()
    {
        if (light2D == null)
            return;
        
        flickerTimer = 0f;
        fromIntensity = light2D.intensity;
        toIntensity = Random.Range(fllickerIntencityMin, fllickerIntencityMax);
    }
    void PickNewRadiusTarget()
    {
        if (light2D == null)
            return;
        radiusTimer = 0f;
        fromRadius = light2D.pointLightOuterRadius; 
        toRadius = Random.Range(fllickerRadiusMin, fllickerRadiusMax);
    }
}
