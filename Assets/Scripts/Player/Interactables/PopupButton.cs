using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class PopupButton : MonoBehaviour, IInteractable
{
    [SerializeField] private PlayableAsset enterAnimationAsset;
    [SerializeField] private PlayableAsset exitAnimationAsset;
    [SerializeField] private HomeScreenBTNPress _BTNPress;
    [SerializeField] private bool _Quit;
    [SerializeField] private string sceneName;
    [SerializeField] private float loadSceneDelay;
    [SerializeField] private bool dontLoadScene;
    [SerializeField] private UnityEvent onTrigger;
    [SerializeField] private bool OneTimeUse = false;
    
    
    private PlayableDirector playableDirector;
    private PlayerInteractor playerInteractor;

    float time;
    private bool _Activated = false;
    private bool used = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playableDirector = GetComponent<PlayableDirector>();
        time = loadSceneDelay;
    }

    
    public void Activate(GameObject gameObject)
    {
        if (used)
            return;
        OnTrigger();
        _BTNPress.PlayBTNAnimation();
        if (dontLoadScene == true)
            return;
        _Activated = true;
        Debug.Log(gameObject.name + " is activated");
    }

    private void OnTrigger()
    {
        Debug.LogWarning(gameObject.name + " is about to invoke onTrigger");

        Debug.LogWarning("onTrigger persistent listener count: " + onTrigger.GetPersistentEventCount());

        onTrigger?.Invoke();
        
        if (OneTimeUse)
            used = true;
        Debug.LogWarning(gameObject.name + " finished invoking onTrigger");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other.name);
        if (!other.CompareTag("Player"))
            return;
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
            if (_Quit)
                LoadSceneManager.Instance.ExitGame();
            LoadSceneManager.Instance.SetSceneName(sceneName);
            if (LoadSceneManager.Instance == null)
                SceneManager.LoadScene(sceneName);
            time -= Time.deltaTime;
            if (time <= 0)
            {
                LoadSceneManager.Instance.LoadScene();
//                Debug.Log(loadScene.name);
            }
           
        }
     
    }
}
