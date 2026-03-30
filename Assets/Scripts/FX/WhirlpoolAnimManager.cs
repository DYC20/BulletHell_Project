using System;
using UnityEngine;
using DG.Tweening;

public class WhirlpoolAnimManager : MonoBehaviour
{
    private Renderer renderer;
    private MaterialPropertyBlock mpb;
    private Sequence activeSequence;
    
    [Header("GO Transforms Settings")]
    [SerializeField] private Vector3 endScale = new Vector3(3f, 3f, 3f);
    [SerializeField] private Ease scaleEase = Ease.Linear;

    [Header("Material Settings")] 
    [SerializeField] private Color startColor = Color.white;
    [SerializeField] private Color endColor = Color.black;
    [SerializeField] private float scaleDuration = 0.5f;
    
    [Header("Twirl 01")] 
    [SerializeField] private float DistortionEndAmount_01 = 0.2f;
    [SerializeField] private float TwirlEndAmount_01 = 26f;
    [SerializeField] private float RotationEndSpeed_01 = 10f;
    [SerializeField] private float NoiseEndAmount_01 = 10f;
    
    [Header("Twirl 02")] 
    [SerializeField] private float DistortionEndAmount_02 = 0.3f;
    [SerializeField] private float TwirlEndAmount_02 = -80f;
    [SerializeField] private float RotationEndSpeed_02 = 5f;
    [SerializeField] private float NoiseEndAmount_02 = 10f;
    
    [SerializeField] private float materialDuration = 0.5f;
    
    public static class ShaderIDs
    {
        public static readonly int matColor = Shader.PropertyToID("_Color");
        
        public static readonly int DistortionAmount = Shader.PropertyToID("_Distortion_Amount");
        public static readonly int TwirlAmount = Shader.PropertyToID("_Twirl_Amount");
        public static readonly int RotationSpeed = Shader.PropertyToID("_Rotation_Speed");
        public static readonly int NoiseAmount = Shader.PropertyToID("_Noise_Amount");
        
        public static readonly int DistortionAmount_02 = Shader.PropertyToID("_Distortion_Amount_2");
        public static readonly int TwirlAmount_02 = Shader.PropertyToID("_Twirl_Amount_2");
        public static readonly int RotationSpeed_02 = Shader.PropertyToID("_Rotation_Speed_2");
        public static readonly int NoiseAmount_02 = Shader.PropertyToID("_Noise_Amount_2");
    }

    private void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();
        mpb = new MaterialPropertyBlock();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.localScale = Vector3.zero;
        renderer.GetPropertyBlock(mpb);
        
        mpb.SetColor(ShaderIDs.matColor, startColor);

        mpb.SetFloat(ShaderIDs.DistortionAmount, 0f);
        mpb.SetFloat(ShaderIDs.TwirlAmount, 0f);
        mpb.SetFloat(ShaderIDs.RotationSpeed, 0f);
        mpb.SetFloat(ShaderIDs.NoiseAmount, 0f);
        
        mpb.SetFloat(ShaderIDs.DistortionAmount_02, 0f);
        mpb.SetFloat(ShaderIDs.TwirlAmount_02,0f);
        mpb.SetFloat(ShaderIDs.RotationSpeed_02, 0f);
        mpb.SetFloat(ShaderIDs.NoiseAmount_02, 0f);
        
        renderer.SetPropertyBlock(mpb);
    }

    public void PlaySequence()
    {
        
        activeSequence?.Kill();
        
        activeSequence = DOTween.Sequence();
        
        activeSequence.Join(transform.DOScale(endScale, scaleDuration).SetEase(scaleEase));
        activeSequence.Join(TweenColor(ShaderIDs.matColor, startColor, endColor, materialDuration).SetEase(scaleEase));

        activeSequence.Join(TweenShaderFloat(ShaderIDs.DistortionAmount,    0f, DistortionEndAmount_01, materialDuration));
        activeSequence.Join(TweenShaderFloat(ShaderIDs.TwirlAmount,         0f, TwirlEndAmount_01,      materialDuration));
        activeSequence.Join(TweenShaderFloat(ShaderIDs.RotationSpeed,       0f, RotationEndSpeed_01,    materialDuration));
        activeSequence.Join(TweenShaderFloat(ShaderIDs.NoiseAmount,         0f, NoiseEndAmount_01,      materialDuration));

        activeSequence.Join(TweenShaderFloat(ShaderIDs.DistortionAmount_02, 0f, DistortionEndAmount_02, materialDuration));
        activeSequence.Join(TweenShaderFloat(ShaderIDs.TwirlAmount_02,      0f, TwirlEndAmount_02,      materialDuration));
        activeSequence.Join(TweenShaderFloat(ShaderIDs.RotationSpeed_02,    0f, RotationEndSpeed_02,    materialDuration));
        activeSequence.Join(TweenShaderFloat(ShaderIDs.NoiseAmount_02,      0f, NoiseEndAmount_02,      materialDuration));

        activeSequence.SetLink(gameObject);
    }
    
    private void SetFloat(int propertyID, float value)
    {
        renderer.GetPropertyBlock(mpb);
        mpb.SetFloat(propertyID, value);
        renderer.SetPropertyBlock(mpb);
    }
    
    private void SetColor(int propertyID, Color value)
    {
        renderer.GetPropertyBlock(mpb);
        mpb.SetColor(propertyID, value);
        renderer.SetPropertyBlock(mpb);
    }
    private Tween TweenShaderFloat(int propertyID, float from, float to, float duration, Ease ease = Ease.Linear)
    {
        float current = from;

        SetFloat(propertyID, current);

        return DOTween.To(
                () => current,
                x =>
                {
                    current = x;
                    SetFloat(propertyID, current);
                },
                to,
                duration)
            .SetEase(ease);
    }
    public Tween TweenColor( int colorID,Color from, Color to, float duration)
    {
        Color current = from;
        SetColor(colorID, current);

        return DOTween.To(
            () => current,
            x =>
            {
                current = x;
                SetColor(colorID, current);
            },
            to,
            duration
        ).SetLink(gameObject);
    }
    private void OnDestroy()
    {
        activeSequence?.Kill();
    }
}
