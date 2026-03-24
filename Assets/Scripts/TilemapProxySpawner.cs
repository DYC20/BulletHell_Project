using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapProxySpawner : MonoBehaviour
{
    [SerializeField] private Tilemap[] tilemaps;
    [SerializeField] private WhirlpoolManager manager;

    [Header("Sampling")]
    [SerializeField] private int tileStep = 1;

    [Header("Proxy")]
    [SerializeField] private GameObject proxyPrefab;

    public void SpawnProxies()
    {
        if (manager == null || proxyPrefab == null || tilemaps == null)
            return;

        foreach (var tilemap in tilemaps)
        {
            if (tilemap == null)
                continue;

            BoundsInt bounds = tilemap.cellBounds;
            TilemapRenderer tilemapRenderer = tilemap.GetComponent<TilemapRenderer>();

            for (int z = bounds.zMin; z < bounds.zMax; z++)
            {
                for (int x = bounds.xMin; x < bounds.xMax; x += tileStep)
                {
                    for (int y = bounds.yMin; y < bounds.yMax; y += tileStep)
                    {
                        Vector3Int cellPos = new Vector3Int(x, y, z);

                        if (!tilemap.HasTile(cellPos))
                            continue;

                        Sprite sprite = tilemap.GetSprite(cellPos);
                        if (sprite == null)
                            continue;

                        Vector3 worldPos = tilemap.GetCellCenterWorld(cellPos);

                        GameObject proxy = Instantiate(proxyPrefab, worldPos, Quaternion.identity);

                        SpriteRenderer sr = proxy.GetComponent<SpriteRenderer>();
                        if (sr == null)
                        {
                            Debug.LogWarning($"Proxy prefab '{proxyPrefab.name}' is missing a SpriteRenderer.", proxy);
                            Destroy(proxy);
                            continue;
                        }

                        sr.sprite = sprite;
                        sr.color = tilemap.GetColor(cellPos);

                        if (tilemapRenderer != null)
                        {
                            sr.sortingLayerID = tilemapRenderer.sortingLayerID;
                            sr.sortingOrder = tilemapRenderer.sortingOrder + z;
                        }

                        manager.RegisterExternalTarget(proxy.transform);
                    }
                }
            }

            tilemap.gameObject.SetActive(false);
        }
    }
}