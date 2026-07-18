using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ShockWave : MonoBehaviour
    {
  [Header("Shader Property Names")]
    [SerializeField] private string sizeProperty = "_Size";
    [SerializeField] private string strengthProperty = "_Strength";
    [SerializeField] private string distanceProperty = "_DistanceFromCenter";
    [Header("Animation")]
    [SerializeField] private float duration = 0.6f;

    [SerializeField] private float startSize = 0f;
    [SerializeField] private float endSize = 1f;

    [SerializeField] private float startStrength = 1f;
    [SerializeField] private float endStrength = 0f;

    [SerializeField] private float startDistance = 0f;
    [SerializeField] private float endDistance = 5f;

    [Header("Optional Curves")]
    [SerializeField] private AnimationCurve sizeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve strengthCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve distanceCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    private Renderer _renderer;
    private MaterialPropertyBlock _mpb;
    private Coroutine _playRoutine;

    private int _sizeID;
    private int _strengthID;
    private int _distanceID;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _mpb = new MaterialPropertyBlock();

        _sizeID = Shader.PropertyToID(sizeProperty);
        _strengthID = Shader.PropertyToID(strengthProperty);
        _distanceID = Shader.PropertyToID(distanceProperty);
    }

    /*private void Update()
    {
        if (_mpb.GetFloat(_sizeID) == endSize &&
            _mpb.GetFloat(_strengthID) == endStrength &&
            _mpb.GetFloat(_distanceID) == endDistance)
        {
            Destroy(gameObject);
            
            _mpb.SetFloat(_sizeID, startSize);
            _mpb.SetFloat(_strengthID, startStrength);
            _mpb.SetFloat(_distanceID, startDistance);
           
        }
    } */

    /// <summary>
    /// Call this from another script and pass the world position of the source object.
    /// </summary>
    public void PlayShockwave()
    {
        if (_playRoutine != null)
            StopCoroutine(_playRoutine);

        _playRoutine = StartCoroutine(AnimateShockwave());
    }

    private IEnumerator AnimateShockwave()
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);

            float currentSize = Mathf.Lerp(startSize, endSize, sizeCurve.Evaluate(t));
            float currentStrength = Mathf.Lerp(startStrength, endStrength, strengthCurve.Evaluate(t));
            float currentDistance = Mathf.Lerp(startDistance, endDistance, distanceCurve.Evaluate(t));

            _renderer.GetPropertyBlock(_mpb);

            _mpb.SetFloat(_sizeID, currentSize);
            _mpb.SetFloat(_strengthID, currentStrength);
            _mpb.SetFloat(_distanceID, currentDistance);

            _renderer.SetPropertyBlock(_mpb);

            yield return null;
            
            ApplyShockwaveValues(currentSize, currentStrength, currentDistance);
        }

        // Force exact final values
        

        _playRoutine = null;
        _mpb.SetFloat(_sizeID, 0f);
        _mpb.SetFloat(_strengthID, 0f);
        //Destroy(gameObject);
    }
    private void ApplyShockwaveValues(float size, float strength, float distance)
    {
        _renderer.GetPropertyBlock(_mpb);

        _mpb.SetFloat(_sizeID, size);
        _mpb.SetFloat(_strengthID, strength);
        _mpb.SetFloat(_distanceID, distance);

        _renderer.SetPropertyBlock(_mpb);
    }
}
