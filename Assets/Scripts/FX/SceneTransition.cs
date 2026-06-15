using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private float startValue = -0.5f;
    [SerializeField] private float endValue = 1f;
    [SerializeField] private float distanceFromCamera = 5f;
    
    //private float reverseStartValue;
    //private float reverseEndValue;
    
    private Image renderer;
    private Material runtimeMaterial;
    
    private static readonly int ValueID = Shader.PropertyToID("_Value");
    /*
    public void RefreshCamera()
    {
        currentCamera = Camera.main;

        if (currentCamera == null)
        {
            Debug.LogWarning("No Camera.main found for scene transition.");
            return;
        }

        AlignToCamera();
    }

    private void LateUpdate()
    {
        if (currentCamera == null) return;

        AlignToCamera();
    }

    private void AlignToCamera()
    {
        transform.position =
            currentCamera.transform.position +
            currentCamera.transform.forward * distanceFromCamera;

        transform.rotation = currentCamera.transform.rotation;
    }
    */
    private void Awake()
    {
        // Create a unique material instance for this UI Image
        renderer = GetComponent<Image>();
        
        runtimeMaterial = Instantiate(renderer.material);
        renderer.material = runtimeMaterial;

        SetValue(startValue);
    }
    
    private void SetValue(float value)
    {
        runtimeMaterial.SetFloat(ValueID, value);
    }
    
    public IEnumerator TransitionOutRoutine()
    {
        yield return AnimateValue(startValue, endValue);
    }

    public IEnumerator TransitionInRoutine()
    {
        yield return AnimateValue(endValue, startValue);
    }

    private IEnumerator AnimateValue(float valueA, float valueB)
    {
        float elapsedTime = 0;
        
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            
            float t = Mathf.Clamp01(elapsedTime / transitionDuration);
            float vlue = Mathf.Lerp(valueA, valueB, t);
            
            SetValue(vlue);
            Debug.Log(vlue);
            yield return null;
        }
        SetValue(valueB);
    }
    
    
    
}
