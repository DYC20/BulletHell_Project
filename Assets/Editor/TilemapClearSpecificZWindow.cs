#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapClearSpecificZWindow : EditorWindow
{
    private int zToClear = 0;

    [MenuItem("Tools/Tilemap/Clear Specific Z Position")]
    private static void Open()
    {
        GetWindow<TilemapClearSpecificZWindow>("Clear Tilemap Z");
    }

    private void OnGUI()
    {
        zToClear = EditorGUILayout.IntField("Z Position To Clear", zToClear);

        if (GUILayout.Button("Clear Selected Tilemaps At This Z"))
        {
            ClearSelectedTilemapsAtZ(zToClear);
        }
    }

    private static void ClearSelectedTilemapsAtZ(int z)
    {
        Tilemap[] tilemaps = Selection.gameObjects.Length > 0
            ? Selection.activeGameObject.GetComponentsInChildren<Tilemap>(true)
            : new Tilemap[0];

        if (tilemaps.Length == 0)
        {
            Debug.LogWarning("Select a Tilemap or its parent Grid first.");
            return;
        }

        foreach (Tilemap tilemap in tilemaps)
        {
            Undo.RecordObject(tilemap, $"Clear Tilemap Z {z}");

            BoundsInt bounds = tilemap.cellBounds;
            int removed = 0;

            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);

                    if (tilemap.HasTile(pos))
                    {
                        tilemap.SetTile(pos, null);
                        removed++;
                    }
                }
            }

            tilemap.CompressBounds();
            EditorUtility.SetDirty(tilemap);

            Debug.Log($"{tilemap.name}: removed {removed} tiles at Z {z}.", tilemap);
        }
    }
}
#endif