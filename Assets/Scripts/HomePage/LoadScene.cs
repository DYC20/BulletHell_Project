using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Unity.Collections;

public class LoadScene : MonoBehaviour
{
    [SerializeField] private string sceneName;
    [SerializeField] private List<ParticleSystem> pressFX;
    
    [Header("BTN Animation Settings")]
    [SerializeField] private float scaleDuration;
    [SerializeField] private Ease ease;
    
    private RectTransform buttonTF;
    private Sequence seq;

    private void Start()
    {
        buttonTF = GetComponent<RectTransform>();
        
    }
    
    public void LoadSelectedScene()
    {
        SceneManager.LoadScene(sceneName);
    }

    public void PlayBTNAnimation()
    {
        ButtonAnimation();
    }

    private void ButtonAnimation()
    {
        seq?.Kill();
        
        seq = DOTween.Sequence();

        seq.Append(buttonTF.DOScale(0.5f, scaleDuration).SetEase(ease))
            .Append(buttonTF.DOScale(1.5f, scaleDuration).SetEase(ease))
        
            .JoinCallback(() =>
                {
                    foreach (var ps in pressFX)
                        ps.Play();
                })
            .Append(buttonTF.DOScale(1, scaleDuration).SetEase(ease));
    }
}
