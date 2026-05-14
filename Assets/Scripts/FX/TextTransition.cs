using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Graphic))]
public class UIMaterialTransition : MonoBehaviour, IMaterialModifier
{
    [Header("Transition")]
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private float visibleValue = 0f;
    [SerializeField] private float hiddenValue = 1f;

    private Graphic graphic;
    private Material runtimeMaterial;
    private Material lastBaseMaterial;

    private Coroutine transitionRoutine;

    private float transitionValue;

    private static readonly int TransitionValueID = Shader.PropertyToID("_Transition_Value");

    private void Awake()
    {
        graphic = GetComponent<Graphic>();

        // At 1 your material is hidden.
        transitionValue = hiddenValue;

        graphic.SetMaterialDirty();
    }

    public void StartTransition()
    {
        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(TransitionRoutine(hiddenValue, visibleValue));
    }

    public void ReverseTransition()
    {
        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(TransitionRoutine(visibleValue, hiddenValue));
    }

    private IEnumerator TransitionRoutine(float from, float to)
    {
        float time = 0f;

        while (time < transitionDuration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / transitionDuration);
            transitionValue = Mathf.Lerp(from, to, t);

            // This tells the UI system:
            // "Recalculate the materialForRendering using GetModifiedMaterial."
            graphic.SetMaterialDirty();

            yield return null;
        }

        transitionValue = to;
        graphic.SetMaterialDirty();

        transitionRoutine = null;
    }

    public Material GetModifiedMaterial(Material baseMaterial)
    {
        if (baseMaterial == null)
            return null;

        // If Unity gives us a different base material, usually because of Mask/stencil changes,
        // recreate our runtime instance from that base material.
        if (runtimeMaterial == null || lastBaseMaterial != baseMaterial)
        {
            if (runtimeMaterial != null)
                Destroy(runtimeMaterial);

            runtimeMaterial = new Material(baseMaterial);
            runtimeMaterial.name = baseMaterial.name + " Runtime UI Instance";

            lastBaseMaterial = baseMaterial;
        }

        if (runtimeMaterial.HasProperty(TransitionValueID))
        {
            runtimeMaterial.SetFloat(TransitionValueID, transitionValue);
        }
        else
        {
            Debug.LogWarning(
                $"{nameof(UIMaterialTransition)}: Material '{runtimeMaterial.name}' does not have property _Transition_Value",
                this
            );
        }

        return runtimeMaterial;
    }

    private void OnDestroy()
    {
        if (runtimeMaterial != null)
            Destroy(runtimeMaterial);
    }
}