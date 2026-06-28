using System.Collections;
using UnityEngine;
using TMPro;

public class TMPDilate : MonoBehaviour
{
    [SerializeField] private float startValue = 0f;
    [SerializeField] private float endValue = 0.5f;
    [SerializeField] private float duration = 0.5f;

    private Material material;
    private TMP_Text text;
    private Coroutine dilateRoutine;

    private static readonly int FaceDilateID = Shader.PropertyToID("_FaceDilate");

    private void Awake()
    {
        text = GetComponent<TMP_Text>();

        // Create unique material instance for this TMP only
        material = new Material(text.fontSharedMaterial);

        // Assign it back to this TMP
        text.fontSharedMaterial = material;
        
        material.SetFloat(FaceDilateID, -1);
        text.SetMaterialDirty();

        Debug.Log("TMP material assigned to: " + gameObject.name);
        Debug.Log("Material has _FaceDilate: " + material.HasProperty(FaceDilateID));
    }

    private IEnumerator LerpDilate(float from, float to)
    {

        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(time / duration);
            float value = Mathf.Lerp(from, to, t);

            material.SetFloat(FaceDilateID, value);
            text.SetMaterialDirty();

            yield return null;
        }

        material.SetFloat(FaceDilateID, to);
        text.SetMaterialDirty();

        Debug.Log("Finished TMP dilate on " + gameObject.name + " value: " + to);
    }

    public void StartLerpDilate()
    {
        if (dilateRoutine != null)
            StopCoroutine(dilateRoutine);

        dilateRoutine = StartCoroutine(LerpDilate(startValue, endValue));
    }

    public void StartReverseLerpDilate()
    {
        if (dilateRoutine != null)
            StopCoroutine(dilateRoutine);

        dilateRoutine = StartCoroutine(LerpDilate(endValue, startValue));
    }
}