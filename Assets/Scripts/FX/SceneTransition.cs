using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private float startValue = -0.5f;
    [SerializeField] private float endValue = 1f;
    
    private float reverseStartValue;
    private float reverseEndValue;
    
    private SpriteRenderer spriteRenderer;
    private Material material;
    private MaterialPropertyBlock mpb;
    
    private static readonly int VlueID = Shader.PropertyToID("_Value");

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        material = spriteRenderer.material;
  ;
        mpb = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(mpb);

        reverseStartValue = startValue * -1;
        reverseEndValue = endValue * -1;
    }
    
    public IEnumerator TransitionOutRoutine()
    {
        yield return AnimateValue(startValue, endValue);
    }

    public IEnumerator TransitionInRoutine()
    {
        yield return AnimateValue(reverseStartValue, reverseEndValue);
    }

    private IEnumerator AnimateValue(float valueA, float valueB)
    {
        float elapsedTime = 0;
        
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            
            float t = Mathf.Clamp01(elapsedTime / transitionDuration);
            float vlue = Mathf.Lerp(valueA, valueB, t);
            
            mpb.SetFloat(VlueID, vlue);
            spriteRenderer.SetPropertyBlock(mpb);
            yield return null;
        }
        material.SetFloat(VlueID, endValue);
        spriteRenderer.SetPropertyBlock(mpb);
    }
}
