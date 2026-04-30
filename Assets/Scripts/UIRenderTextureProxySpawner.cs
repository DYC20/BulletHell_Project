using UnityEngine;
using UnityEngine.UI;

public class UIRenderTextureProxySpawner : MonoBehaviour
{
    [Header("Capture")]
    [SerializeField] private Camera uiCaptureCamera;
    [SerializeField] private Canvas canvasToCapture;
    [SerializeField] private Camera worldCamera;

    [Header("Tiles")]
    [SerializeField] private int columns = 8;
    [SerializeField] private int rows = 5;
    [SerializeField] private string sortingLayerName = "UI";
    [SerializeField] private int sortingOrder = 100;

    [Header("Whirlpool")]
    [SerializeField] private WhirlpoolManager manager;

    [Header("Options")]
    [SerializeField] private bool hideOriginalCanvas = true;

    public void SpawnUITextureTiles()
    {
        if (manager == null)
            manager = WhirlpoolManager.Instance;

        if (worldCamera == null)
            worldCamera = Camera.main;

        if (uiCaptureCamera == null || canvasToCapture == null || worldCamera == null || manager == null)
        {
            Debug.LogWarning("UIRenderTextureProxySpawner missing references.");
            return;
        }

        int width = Screen.width;
        int height = Screen.height;

        RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        rt.Create();

        uiCaptureCamera.targetTexture = rt;
        uiCaptureCamera.clearFlags = CameraClearFlags.SolidColor;
        uiCaptureCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);

        uiCaptureCamera.Render();

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D capturedTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        capturedTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        capturedTexture.Apply();

        RenderTexture.active = previous;
        uiCaptureCamera.targetTexture = null;

        rt.Release();
        Destroy(rt);

        SpawnTilesFromTexture(capturedTexture);

        if (hideOriginalCanvas)
            canvasToCapture.gameObject.SetActive(false);
    }

    private void SpawnTilesFromTexture(Texture2D texture)
    {
        int tileWidth = texture.width / columns;
        int tileHeight = texture.height / rows;

        float pixelsPerUnit = texture.height / (worldCamera.orthographicSize * 2f);

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                Rect spriteRect = new Rect(
                    x * tileWidth,
                    y * tileHeight,
                    tileWidth,
                    tileHeight
                );

                Sprite sprite = Sprite.Create(
                    texture,
                    spriteRect,
                    new Vector2(0.5f, 0.5f),
                    pixelsPerUnit
                );

                float screenX = spriteRect.x + spriteRect.width * 0.5f;
                float screenY = spriteRect.y + spriteRect.height * 0.5f;

                Vector3 worldPos = worldCamera.ScreenToWorldPoint(
                    new Vector3(screenX, screenY, Mathf.Abs(worldCamera.transform.position.z))
                );

                worldPos.z = 0f;

                GameObject tile = new GameObject("UI_Texture_Tile");
                tile.transform.position = worldPos;

                SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingLayerName = sortingLayerName;
                sr.sortingOrder = sortingOrder;

                manager.RegisterExternalTarget(tile.transform);
            }
        }
    }
}