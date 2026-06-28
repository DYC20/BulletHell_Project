using UnityEngine;
using System.Collections.Generic;
public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    public bool IsPaused { get; private set;}
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void PauseGame()
    {
        IsPaused = true;
        Time.timeScale = 0;
        InputManager.playerInput.SwitchCurrentActionMap("UI");
        Debug.Log("input map: " + InputManager.playerInput.currentActionMap);
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1;
        InputManager.playerInput.SwitchCurrentActionMap("Player");
    }
    
}
