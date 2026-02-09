using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightFllicker : MonoBehaviour
{
    [SerializeField] private Light2D light;
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
        PickNewFlickerTarget();
        PickNewRadiusTarget();
    }

    void Update()
    {
        flickerTimer += Time.deltaTime * fllickerSpeed;
        radiusTimer += Time.deltaTime * fllickerRadiusSpeed;

        light.intensity = Mathf.Lerp(fromIntensity, toIntensity, flickerTimer);
        light.pointLightOuterRadius = Mathf.Lerp(fromRadius, toRadius, radiusTimer);

        if (flickerTimer >= 1f)
            PickNewFlickerTarget();
        if (radiusTimer >= 1F)
            PickNewRadiusTarget();
    }

    void PickNewFlickerTarget()
    {
        flickerTimer = 0f;
        fromIntensity = light.intensity;
        toIntensity = Random.Range(fllickerIntencityMin, fllickerIntencityMax);
    }
    void PickNewRadiusTarget()
    {
        radiusTimer = 0f;
        fromRadius = light.pointLightOuterRadius; 
        toRadius = Random.Range(fllickerRadiusMin, fllickerRadiusMax);
    }
}
