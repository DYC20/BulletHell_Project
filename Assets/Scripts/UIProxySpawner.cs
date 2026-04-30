using UnityEngine;
using UnityEngine.UI;

public class UIProxySpawner : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform uiRootToCopy;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private GameObject proxyPrefab;
    [SerializeField] private WhirlpoolManager manager;

    [Header("Options")]
    [SerializeField] private bool hideOriginalUI = true;

    public void SpawnUIProxies()
    {
        if (manager == null)
            manager = WhirlpoolManager.Instance;

        if (canvas == null || uiRootToCopy == null || proxyPrefab == null || manager == null)
            return;

        if (worldCamera == null)
            worldCamera = Camera.main;

        Image[] images = uiRootToCopy.GetComponentsInChildren<Image>(true);

        foreach (Image img in images)
        {
            if (img == null || img.sprite == null)
                continue;

            RectTransform rt = img.rectTransform;

            Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, rt.position);
            Vector3 worldPos = worldCamera.ScreenToWorldPoint(screenPos);
            worldPos.z = 0f;

            GameObject proxy = Instantiate(proxyPrefab, worldPos, rt.rotation);

            SpriteRenderer sr = proxy.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                Destroy(proxy);
                continue;
            }

            sr.sprite = img.sprite;
            sr.color = img.color;

            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);

            float uiWidth = Vector3.Distance(corners[0], corners[3]);
            float spriteWidth = sr.sprite.bounds.size.x;

            if (spriteWidth > 0f)
            {
                float scale = uiWidth / spriteWidth;
                proxy.transform.localScale = Vector3.one * scale;
            }

            manager.RegisterExternalTarget(proxy.transform);
        }

        if (hideOriginalUI)
            uiRootToCopy.gameObject.SetActive(false);
    }
}