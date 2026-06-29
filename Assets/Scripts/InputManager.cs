using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    public bool MenuOpenInput;
    public bool MenuExitInput;
    
    public static PlayerInput playerInput;
    
    private InputAction _menuOpenAction;
    private InputAction _menuExitAction;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        if (instance == null)
            instance = this;
        playerInput = GetComponent<PlayerInput>();
        
        _menuOpenAction = playerInput.actions["MenuOpen"];
        _menuExitAction = playerInput.actions["MenuExit"];
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        MenuOpenInput = _menuOpenAction.WasPressedThisFrame();
        MenuExitInput = _menuExitAction.WasPressedThisFrame();
    }
}
