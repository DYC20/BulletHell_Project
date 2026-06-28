using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class QuitMenu : MonoBehaviour
{
    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField] private PlayableAsset enterMenuPlayable;
    [SerializeField] private PlayableAsset exitMenuPlayable;

    private bool isMenuOpen;

    private void Update()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            ToggleMenu();
        }
    }

    private void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;

        playableDirector.playableAsset = isMenuOpen 
            ? enterMenuPlayable 
            : exitMenuPlayable;

        playableDirector.time = 0;
        playableDirector.Play();

        Debug.Log(isMenuOpen ? "Enter quit menu" : "Exit quit menu");
    }
}