using UnityEngine;
using System.Collections;
using Unity.VisualScripting;


public class MaterialValueLerp : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Material targetMaterial;
    
    [SerializeField] private float startValue = 0f;
    [SerializeField] private float endValue = 1f;
    [SerializeField] private float duration;

    private MaterialPropertyBlock mpb;
    private static readonly int ValueID = Shader.PropertyToID("_Value");
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {

    }

    public void AnimateTransition()
    {
        Initialize();
        StartCoroutine(LerpMaterial());
    }

    public void AnimateTransitionReverse()
    {
        Initialize();
        StartCoroutine(LerpMaterialReverse());
    }
    

    private void Initialize()
    {
                if (spriteRenderer == null)
                    spriteRenderer = GetComponent<SpriteRenderer>();
                if (targetMaterial == null)
                    targetMaterial = spriteRenderer.material;
                
                mpb = new MaterialPropertyBlock();
                spriteRenderer.GetPropertyBlock(mpb);
    }

    private IEnumerator LerpMaterial()
    {
        Debug.LogWarning("LerpMaterial Start");
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            
            
            float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsedTime / duration);
            float value = Mathf.Lerp(startValue, endValue, t);
            
            mpb.SetFloat(ValueID, value);
            spriteRenderer.SetPropertyBlock(mpb);
            
            yield return null;
        }
        mpb.SetFloat(ValueID, endValue);
        spriteRenderer.SetPropertyBlock(mpb);
    }

    private IEnumerator LerpMaterialReverse()
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            
            
            float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsedTime / duration);
            float value = Mathf.Lerp(endValue, startValue, t);
            
            mpb.SetFloat(ValueID, value);
            spriteRenderer.SetPropertyBlock(mpb);
            
            yield return null;
        }
        mpb.SetFloat(ValueID, startValue);
        spriteRenderer.SetPropertyBlock(mpb);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
