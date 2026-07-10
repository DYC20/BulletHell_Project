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
    [SerializeField] private Color flashColor = Color.red;
    
    [Header("Low HealthSettings")]
    [SerializeField] private float _LHAberrationDuration = 0.08f;
    [SerializeField] private float _LHDeformDuration = 0.08f;
    [SerializeField] private float _LHAberrationAmount = 0.2f;
    [SerializeField] private float _LHDeformAmount = 0.2f;
    [SerializeField] private float _LHDFlashDurationIn = 0.08f;
    [SerializeField] private float _LHDFlashDurationOut = 0.08f;
    [SerializeField] private float _LHDIntervalDuration = 0.2f;

    private SpriteRenderer _renderer;
    private MaterialPropertyBlock _mpb;
    private Sequence seq;
    private Sequence seq_02;
    
    private bool _isFlashing = false;

    private static readonly int AberrationValueId = Shader.PropertyToID("_Aberration_Value");
    private static readonly int DeformPosId = Shader.PropertyToID("_DeformPos");

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _mpb = new MaterialPropertyBlock();
    }

    public void PlayFlash()
    {
        if (_isFlashing) return;
        StartCoroutine(HitAnimation());
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
                   //seq.Join(_renderer.DOColor(flashColor, _LHDeformDuration));
                   seq.Append(SetValue(_LHAberrationAmount, -_LHAberrationAmount, _LHAberrationDuration, AberrationValueId));
                   seq.Append(SetValue(-_LHAberrationAmount, 0f, _LHAberrationDuration, AberrationValueId));
                   //seq.Join(_renderer.DOColor(Color.white, _LHDeformDuration));
                   seq.SetLoops(-1, LoopType.Yoyo); 
                   
            seq_02 = DOTween.Sequence();
            //seq_02.Join(seq);
            seq_02.Append(_renderer.DOColor(flashColor, _LHDFlashDurationIn)).SetEase(Ease.InOutExpo);
            seq_02.AppendInterval(_LHDIntervalDuration);
            seq_02.Append(_renderer.DOColor(Color.white, _LHDFlashDurationOut)).SetEase(Ease.OutCubic);
            seq_02.SetLoops(-1, LoopType.Yoyo); 
        }
        else
        {
            seq?.Kill();
            
            seq = DOTween.Sequence();
            seq.Append(SetValue(_LHAberrationAmount, 0f, _LHAberrationDuration, AberrationValueId));
            seq.Join(SetValue(_LHDeformAmount, 0f, _LHDeformDuration, DeformPosId));
            Debug.Log("AberrationValueId: " + AberrationValueId);
            Debug.Log("DeformPosId:" + DeformPosId);
            Debug.Log("Health:" + health);
        }
    }
    private IEnumerator HitAnimation()
    {
        for (int i = 0; i < flashCount; i++)
        {
            _isFlashing = true;
            seq?.Kill();
        
            seq = DOTween.Sequence();
            seq.Append(SetValue(0f, aberrationAmount, aberrationDuration, AberrationValueId));
                seq.Join(SetValue(0f, deformAmount, deformDuration, DeformPosId));
                seq.Append(SetValue(aberrationAmount, -aberrationAmount, aberrationDuration, AberrationValueId));
                seq.Append(SetValue(-aberrationAmount, 0f, aberrationDuration, AberrationValueId));
                
                seq_02 = DOTween.Sequence();
                //seq_02.Join(seq);
                seq_02.Append(_renderer.DOColor(flashColor, _LHDFlashDurationIn)).SetEase(Ease.InOutExpo);
                seq_02.AppendInterval(_LHDIntervalDuration);
                seq_02.Append(_renderer.DOColor(Color.white, _LHDFlashDurationOut)).SetEase(Ease.OutCubic);
                
                yield return new WaitForSeconds(aberrationDuration*2f);
        }
        _isFlashing = false;
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
