using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField] private PlayableAsset enterMenuPlayable;
    [SerializeField] private PlayableAsset exitMenuPlayable;


    private void Update()
    {
        if (InputManager.instance.MenuOpenInput && !PauseManager.Instance.IsPaused)
        {
            Pause();
            return;
        }

        if (InputManager.instance.MenuExitInput && PauseManager.Instance.IsPaused)
        {
            Unpause();
            return;
        }
    }

    private void Pause()
    {
        PlayTimeline(enterMenuPlayable);
        PauseManager.Instance.PauseGame();
    }



    private void Unpause()
    {
        PauseManager.Instance.ResumeGame();
        PlayTimeline(exitMenuPlayable);
    }
    
    private void PlayTimeline(PlayableAsset playableAsset)
    {
        playableDirector.Stop();

        playableDirector.playableAsset = playableAsset;
        playableDirector.time = 0;

        playableDirector.Evaluate();
        playableDirector.Play();
    }
}
