using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightFllicker : MonoBehaviour
{
    [SerializeField] private Light2D light;
    [SerializeField] private float fllickerTime = 0.2f;
    [SerializeField] private float fllickerRadius = 0.2f;
    [SerializeField] private float fllickerSpeed = 0.2f;
    [SerializeField] private float fllickerIntencityMax = 0.2f;
    [SerializeField] private float fllickerIntencityMin = 0.2f;

    private void Update()
    {
        Fllicker();
    }

    private void Fllicker()
    {
        float fllickerRange1 = Random.Range(fllickerIntencityMin, fllickerIntencityMax);
        float fllickerRange2 = Random.Range(fllickerIntencityMin, fllickerIntencityMax);
        
        float flickerPerform = Mathf.Lerp(fllickerRange1, fllickerRange2, fllickerSpeed * Time.deltaTime);

        light.intensity = flickerPerform;
    }
}
