using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

public class PopupButton : MonoBehaviour, IInteractable
{
    [SerializeField] private PlayableAsset enterAnimationAsset;
    [SerializeField] private PlayableAsset exitAnimationAsset;
    [SerializeField] private LoadScene loadScene;
    [SerializeField] private float loadSceneDelay;
    [SerializeField] private bool dontLoadScene;
    [SerializeField] private UnityEvent onTrigger;
    
    private PlayableDirector playableDirector;
    private PlayerInteractor playerInteractor;

    float time;
    private bool _Activated = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playableDirector = GetComponent<PlayableDirector>();
        time = loadSceneDelay;
    }

    
    public void Activate(GameObject gameObject)
    {
        OnTrigger();
        if (dontLoadScene == true)
            return;
        loadScene.PlayBTNAnimation();
        _Activated = true;
        Debug.Log(gameObject.name + " is activated");
    }

    private void OnTrigger()
    {
        onTrigger?.Invoke();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other.name);
        if (other.CompareTag("Player"))
        {
            Debug.Log("player detected");
            playableDirector.playableAsset = enterAnimationAsset;
            playableDirector.Play();
            if (playableDirector.playableAsset == enterAnimationAsset)
                Debug.Log("assigned playable asset");
            
            playerInteractor = other.gameObject.GetComponentInParent<PlayerInteractor>();
            if (playerInteractor == null)
                Debug.Log("player interactor is NULL");
            playerInteractor.Interactable = true;
            Debug.Log("player interactor status: " + playerInteractor.Interactable);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playableDirector.playableAsset = exitAnimationAsset;
            playableDirector.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("_Activated status: " + _Activated);
        if (_Activated)
        {
            time -= Time.deltaTime;
            if (time <= 0)
            {
                loadScene.LoadSelectedScene();
//                Debug.Log(loadScene.name);
            }
        }
    }
}
