using UnityEngine;

public class CursorController : MonoBehaviour
{
    [SerializeField] private Texture2D cursorTexture;

    // hotspot is the "click point" of the cursor
    // usually center or tip
    [SerializeField] private Vector2 hotspot;

    private void Start()
    {
        Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
    }
}