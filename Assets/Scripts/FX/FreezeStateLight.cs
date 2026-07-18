using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class FreezeStateLight : MonoBehaviour
{
    [SerializeField] private Light2D freezeLight;
    [SerializeField] private VisualEffect freezeEffect;

    [Header("Timing")]
    [SerializeField] private float lightDuration = 0.12f;

    [Header("Light Values")]
    [SerializeField] private float startRadius = 2.5f;
    [SerializeField] private float endRadius = 0.5f;
    [SerializeField] private float innerRadiusOffset = 0.7f;
    [SerializeField] private AnimationCurve radiusCurve;

    [Header("Preview")]
    [SerializeField] private bool enablePreviewInEditMode = true;
    [SerializeField] private bool autoPreview = true;
    [SerializeField, Range(0f, 1f)] private float previewTime = 0f;
    
    [ContextMenu("Stop Preview")]
    private void StopPreview()
    {
        enablePreviewInEditMode = false;

        if (freezeLight != null)
            freezeLight.enabled = false;
    }

    private Coroutine lightRoutine;

    private void OnEnable()
    {
        if (!Application.isPlaying)
        {
            if (enablePreviewInEditMode)
                ApplyPreview();
        }
        else
        {
            ResetLight();
        }
    }

    private void OnDisable()
    {
        if (lightRoutine != null)
        {
            StopCoroutine(lightRoutine);
            lightRoutine = null;
        }

        // Avoid changing scene lights during editor object deletion,
        // recompilation, prefab editing, etc.
        if (Application.isPlaying)
            ResetLight();
    }

    private void Start()
    {
        if (!Application.isPlaying) return;

        freezeLight.enabled = false;
        freezeLight.pointLightOuterRadius = 0f;
        freezeLight.pointLightInnerRadius = 0f;

        if (freezeEffect != null)
            PlayEffect();
    }

    private void Update()
    {
        if (Application.isPlaying || freezeLight == null)
            return;

        if (!enablePreviewInEditMode)
            return;

        if (autoPreview)
        {
            float safeDuration = Mathf.Max(0.0001f, lightDuration);

            previewTime += Time.deltaTime / safeDuration;
            previewTime %= 1f;

#if UNITY_EDITOR
            SceneView.RepaintAll();
#endif
        }

        ApplyPreview();
    }


    public void PlayEffect()
    {
        //transform.position = worldPosition;

        if (!Application.isPlaying)
        {
            previewTime = 0f;
            ApplyPreview();
            return;
        }

        if (lightRoutine != null)
            StopCoroutine(lightRoutine);

        lightRoutine = StartCoroutine(FlashLight());
    }

    private System.Collections.IEnumerator FlashLight()
    {
        //freezeLight.transform.position = transform.position;
        freezeLight.enabled = true;
       // freezeLight.intensity = 1f;

        float t = 0f;
        float safeDuration = Mathf.Max(0.0001f, lightDuration);

        while (t < safeDuration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / safeDuration);

            ApplyLightAtTime(normalized);

            yield return null;
        }

        //freezeLight.intensity = 0f;
        freezeLight.enabled = false;
        lightRoutine = null;
    }

    private void ApplyPreview()
    {
        if (freezeLight == null || !enablePreviewInEditMode)
            return;

        freezeLight.enabled = true;
        ApplyLightAtTime(Mathf.Clamp01(previewTime));
    }
    private void ResetLight()
    {
        if (freezeLight == null)
            return;

        freezeLight.enabled = false;
        freezeLight.pointLightOuterRadius = 0f;
        freezeLight.pointLightInnerRadius = 0f;
    }

    private void ApplyLightAtTime(float normalized)
    {
        float curveValue = radiusCurve.Evaluate(normalized);

        freezeLight.pointLightOuterRadius =
            Mathf.Lerp(startRadius, endRadius, curveValue);

        freezeLight.pointLightInnerRadius =
            Mathf.Lerp(startRadius - innerRadiusOffset, endRadius - innerRadiusOffset, curveValue);
    }
}
