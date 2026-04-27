using UnityEngine;
using System.Collections;
using DG.Tweening;

public class PlayerHitFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    //[SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float aberrationDuration = 0.08f;
    [SerializeField] private float deformDuration = 0.08f;
    [SerializeField] private int flashCount = 1;
    [SerializeField] private float aberrationAmount = 0.2f;
    [SerializeField] private float deformAmount = 0.2f;
    
    [Header("Low HealthSettings")]
    [SerializeField] private float _LHAberrationDuration = 0.08f;
    [SerializeField] private float _LHDeformDuration = 0.08f;
    [SerializeField] private float _LHAberrationAmount = 0.2f;
    [SerializeField] private float _LHDeformAmount = 0.2f;

    private SpriteRenderer _renderer;
    private MaterialPropertyBlock _mpb;
    private Sequence seq;

    private static readonly int AberrationValueId = Shader.PropertyToID("_Aberration_Value");
    private static readonly int DeformPosId = Shader.PropertyToID("_DeformPos");

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _mpb = new MaterialPropertyBlock();
    }

    public void PlayFlash()
    {
        HitAnimation();
    }

    public void PlayLowHealthMaterial(float health)
    {
        LowHealthAnimation(health);
    }

    private void LowHealthAnimation(float health)
    {
        if (health <= 3f)
        {
           seq?.Kill();
                   
                   seq = DOTween.Sequence();
                   seq.Append(SetValue(0f, _LHAberrationAmount, _LHAberrationDuration, AberrationValueId));
                   seq.Join(SetValue(0f, _LHDeformAmount, _LHDeformDuration, DeformPosId));
                   seq.Append(SetValue(_LHAberrationAmount, -_LHAberrationAmount, _LHAberrationDuration, AberrationValueId));
                   seq.Append(SetValue(-_LHAberrationAmount, 0f, _LHAberrationDuration, AberrationValueId));
                   seq.SetLoops(-1, LoopType.Yoyo); 
        }
        else
        {
            seq?.Kill();
        }
    }
    private void HitAnimation()
    {
        for (int i = 0; i < flashCount; i++)
        {
            seq?.Kill();
        
            seq = DOTween.Sequence();
            seq.Append(SetValue(0f, aberrationAmount, aberrationDuration, AberrationValueId));
                seq.Join(SetValue(0f, deformAmount, deformDuration, DeformPosId));
                seq.Append(SetValue(aberrationAmount, -aberrationAmount, aberrationDuration, AberrationValueId));
                seq.Append(SetValue(-aberrationAmount, 0f, aberrationDuration, AberrationValueId));
        }
    }

    private Tween SetValue(float startValue, float endValue, float duration, int id)
    {
        float current = startValue;

        return DG.Tweening.DOTween.To(
            () => current,
            x =>
            {
                current = x;

                _renderer.GetPropertyBlock(_mpb);
                _mpb.SetFloat(id, x);
                _renderer.SetPropertyBlock(_mpb);
            },
            endValue,
            duration
        );
    }
}
