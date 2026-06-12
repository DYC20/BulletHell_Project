using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

using UnityEngine.UI;

public class MaterialValueLerpUI : MonoBehaviour
{
    [SerializeField] private Image imageRenderer;
    [SerializeField] private Material targetMaterial;
    
    [SerializeField] private float startValue = 0f;
    [SerializeField] private float endValue = 1f;
    [SerializeField] private float duration;
    [SerializeField] private string propertyName = "_Value";

    private Material _RTM;
    private int valueID;
    
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
                if (imageRenderer == null)
                    imageRenderer = GetComponent<Image>();
                valueID = Shader.PropertyToID(propertyName);
                if (targetMaterial == null)
                    targetMaterial = imageRenderer.material;
                if (_RTM == null)
                {
                    _RTM = new Material(targetMaterial);
                    imageRenderer.material = _RTM;
                }
              
    }

    private IEnumerator LerpMaterial()
    {
        BringArtToFront();
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            
            
            float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsedTime / duration);
            float value = Mathf.Lerp(startValue, endValue, t);
            
            _RTM.SetFloat(valueID, value);
            imageRenderer.material = _RTM;
            
            yield return null;
        }
        _RTM.SetFloat(valueID, endValue);
    }

    private IEnumerator LerpMaterialReverse()
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            
            
            float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsedTime / duration);
            float value = Mathf.Lerp(endValue, startValue, t);
            
            _RTM.SetFloat(valueID, value);
            imageRenderer.material = _RTM;
            
            yield return null;
        }
        _RTM.SetFloat(valueID, startValue);
        imageRenderer.material = _RTM;
    }

    private void BringArtToFront()
    {
        Transform activeArt = imageRenderer.transform.parent;

        if (activeArt == null)
            return;

        activeArt.SetAsLastSibling();

        Debug.LogWarning("Art Transform name: " + activeArt.name);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
