using System.Collections;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Unity.Collections;

public class LoadScene : MonoBehaviour
{
    //[SerializeField] private SceneTransition loadSceneTransition;
    [SerializeField] private string sceneName;
    [SerializeField] private List<ParticleSystem> pressFX;
    [SerializeField] private float delayLoadSceneDuration;
    
    [Header("BTN Animation Settings")]
    [SerializeField] private float scaleDuration;
    [SerializeField] private Ease ease;
    [SerializeField] private bool endAtZero;
    
    private RectTransform buttonTF;
    private Transform buttonTransform;
    private Sequence seq;
    

    private void Start()
    {
        buttonTF = GetComponent<RectTransform>();
        if (buttonTF == null)
            buttonTransform =GetComponent<Transform>();
        
    }
    
    //called with signal
    public void LoadSelectedScene()
    {
        StartCoroutine(DelayLoadScene(delayLoadSceneDuration));
    }

    private IEnumerator DelayLoadScene(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
        //DontDestroyOnLoad(loadSceneTransition.gameObject);
    }
    
    public void PlayBTNAnimation()
    {
        ButtonAnimation();
    }

    private void ButtonAnimation()
    {
        if (buttonTF)
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
                    if (endAtZero)
                    {
                        seq.Append(buttonTransform.DOScale(0, scaleDuration).SetEase(ease));
                    }
        }
       
        if (buttonTransform)
        {
            seq?.Kill();
                    
            seq = DOTween.Sequence();
            
            seq.Append(buttonTransform.DOScale(0.5f, scaleDuration).SetEase(ease))
                .Append(buttonTransform.DOScale(1.5f, scaleDuration).SetEase(ease))
                    
                .JoinCallback(() =>
                {
                    foreach (var ps in pressFX)
                        ps.Play();
                })
                .Append(buttonTransform.DOScale(1, scaleDuration).SetEase(ease));
            if (endAtZero)
            {
                seq.Append(buttonTransform.DOScale(0, scaleDuration).SetEase(ease));
            }
        }
        
    }
}
