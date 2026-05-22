using System;
using UnityEngine;
using UnityEngine.Playables;

public class PopupButton : MonoBehaviour, IInteractable
{
    [SerializeField] private PlayableAsset enterAnimationAsset;
    [SerializeField] private PlayableAsset exitAnimationAsset;
    [SerializeField] private PlayableAsset selectAnimationAsset;
    
    private PlayableDirector playableDirector;
    private PlayerInteractor playerInteractor;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playableDirector = GetComponent<PlayableDirector>();
    }

    public void Activate(GameObject gameObject)
    {
        playableDirector.playableAsset = selectAnimationAsset;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playableDirector.playableAsset = enterAnimationAsset;
            
            playerInteractor = other.gameObject.GetComponent<PlayerInteractor>();
            if (playerInteractor != null)
                Debug.Log("player interactor is NULL");
            playerInteractor.Interactable = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playableDirector.playableAsset = exitAnimationAsset;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
