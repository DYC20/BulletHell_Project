using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private float startValue = -0.5f;
    [SerializeField] private float endValue = 1f;
    
    private Material material;
    
    private static readonly int Vlue = Shader.PropertyToID("_Vlue");

    private void Start()
    {
        material = gameObject.GetComponent<Renderer>().material;
        
    }
    
    public void SceneIn()
    {
        StartCoroutine(AnimateVlue(startValue, endValue));
    }

    public void SceneOut()
    {
        startValue = -startValue;
        endValue = -endValue;
        StartCoroutine(AnimateVlue(startValue, endValue));
    }

    private IEnumerator AnimateVlue(float valueA, float valueB)
    {
        float elapsedTime = 0;
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            material.SetFloat(Vlue, Mathf.Lerp(startValue, endValue, elapsedTime));
            yield return null;
        }
    }
}
