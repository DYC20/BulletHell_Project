using System.Collections;
using UnityEngine;

public class PortalTransition : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Material portalMaterial;
    [SerializeField] private Material playerStandardMaterial;
    [SerializeField] private Material StartMaterial;

    [Header("Transition")]
    [SerializeField] private float transitionTime = 1f;
    [SerializeField] private float startValue = 0f;
    [SerializeField] private float endValue = 1f;
    [SerializeField] private bool backToStandardMaterial;

    private Material runtimePortalMaterial;
    private Coroutine transitionRoutine;

    private static readonly int ValueID = Shader.PropertyToID("_Value");

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError($"{name}: Missing SpriteRenderer reference.");
            enabled = false;
            return;
        }

        if (portalMaterial == null)
        {
            Debug.LogError($"{name}: Missing Portal Material reference.");
            enabled = false;
            return;
        }

        if (playerStandardMaterial == null)
        {
            Debug.LogError($"{name}: Missing Player Standard Material reference.");
            enabled = false;
            return;
        }

        //runtimePortalMaterial = new Material(portalMaterial);

        if (!portalMaterial.HasProperty(ValueID))
        {
            Debug.LogError($"{name}: Portal material does not contain property '_Value'.");
            enabled = false;
            return;
        }

        // Player starts with the regular material.
        spriteRenderer.sharedMaterial = StartMaterial;

        SetPortalValue(startValue);
    }

    private void OnDisable()
    {
        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }

        if (spriteRenderer != null && playerStandardMaterial != null)
            spriteRenderer.sharedMaterial = playerStandardMaterial;
    }

    public void StartTransition()
    {
        StartTransition(startValue, endValue);
    }

    public void StartTransition(float fromValue, float toValue)
    {
        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(TransitionRoutine(fromValue, toValue));
    }

    private IEnumerator TransitionRoutine(float fromValue, float toValue)
    {
        //spriteRenderer.sharedMaterial = runtimePortalMaterial;

        SetPortalValue(fromValue);

        float timer = 0f;

        while (timer < transitionTime)
        {
            timer += Time.deltaTime;

            float t = transitionTime <= 0f ? 1f : Mathf.Clamp01(timer / transitionTime);
            float value = Mathf.Lerp(fromValue, toValue, t);

            SetPortalValue(value);

            yield return null;
        }

        SetPortalValue(toValue);

        if (backToStandardMaterial) spriteRenderer.sharedMaterial = playerStandardMaterial;
        

        transitionRoutine = null;
    }

    private void SetPortalValue(float value)
    {
      portalMaterial.SetFloat(ValueID, value);
    }

    private void OnDestroy()
    {
        if (runtimePortalMaterial != null)
            Destroy(runtimePortalMaterial);
    }
}