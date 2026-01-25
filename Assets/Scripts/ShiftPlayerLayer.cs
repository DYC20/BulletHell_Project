using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ShiftPlayerLayer : MonoBehaviour
{
    [SerializeField] GameObject highlandTilemapGO;
    [SerializeField] GameObject lowlandTilemapGO;
    

    private int lowLayer;
    private int highLayer;
    
    private readonly Dictionary<GameObject, int> _insideCount = new Dictionary<GameObject, int>();


    private void Awake()
    {
        lowLayer  = LayerMask.NameToLayer("Player_Low");
        highLayer = LayerMask.NameToLayer("Player_High");

        if (lowLayer < 0 || highLayer < 0)
            Debug.LogError("Missing Player_Low / Player_High layers. Check Tags & Layers.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var playerRoot = GetPlayerRoot(other);
        if (playerRoot == null) return;

        // Increase overlap count
        if (_insideCount.TryGetValue(playerRoot, out int count))
            _insideCount[playerRoot] = count + 1;
        else
            _insideCount[playerRoot] = 1;

        // Only act on the FIRST enter (prevents double toggles from multiple colliders)
        if (_insideCount[playerRoot] != 1)
            return;

        int current = playerRoot.layer;

        if (current == lowLayer)
        {
            SetLayerRecursively(playerRoot, highLayer);

            // Now that we're high, ignore HIGH obstacles while inside (your intended behavior)
            SetIgnorePlayerVsTilemap(playerRoot, highlandTilemapGO, true);
            // If you also need to ensure lowland is NOT ignored:
            SetIgnorePlayerVsTilemap(playerRoot, lowlandTilemapGO, false);

            Debug.LogWarning($"SHIFT: Low -> High | {playerRoot.name}");
        }
        else if (current == highLayer)
        {
            SetLayerRecursively(playerRoot, lowLayer);

            // Now that we're low, ignore LOW obstacles while inside (your intended behavior)
            SetIgnorePlayerVsTilemap(playerRoot, lowlandTilemapGO, true);
            SetIgnorePlayerVsTilemap(playerRoot, highlandTilemapGO, false);

            Debug.LogWarning($"SHIFT: High -> Low | {playerRoot.name}");
        }
        else
        {
            Debug.LogWarning($"ShiftPlayerLayer: Player root '{playerRoot.name}' is on unexpected layer {current} ({LayerMask.LayerToName(current)}).");
        }

        //LogLayers(playerRoot);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var playerRoot = GetPlayerRoot(other);
        if (playerRoot == null) return;

        if (!_insideCount.TryGetValue(playerRoot, out int count))
            return;

        count--;
        if (count <= 0)
        {
            _insideCount.Remove(playerRoot);

            // Only restore ignores when the LAST collider exits
            SetIgnorePlayerVsTilemap(playerRoot, highlandTilemapGO, false);
            SetIgnorePlayerVsTilemap(playerRoot, lowlandTilemapGO, false);

            Debug.LogWarning($"SHIFT ZONE EXIT: Restored ignores | {playerRoot.name}");
        }
        else
        {
            _insideCount[playerRoot] = count;
        }
    }

    // -------- Helpers --------

    private GameObject GetPlayerRoot(Collider2D col)
    {
        // Your setup: Rigidbody2D on root, colliders on children
        if (col.attachedRigidbody != null)
            return col.attachedRigidbody.gameObject;

        // Fallback (won't be stable if it's a child-only setup, but better than null)
        return col.transform.root.gameObject;
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, newLayer);
    }

    private void SetIgnorePlayerVsTilemap(GameObject playerRoot, GameObject tilemapGO, bool ignore)
    {
        if (playerRoot == null || tilemapGO == null) return;

        var playerCols = playerRoot.GetComponentsInChildren<Collider2D>(true);
        var tilemapCols = tilemapGO.GetComponents<Collider2D>(); // Composite/Tilemap/Edge etc.

        foreach (var pc in playerCols)
        foreach (var tc in tilemapCols)
            Physics2D.IgnoreCollision(pc, tc, ignore);
    }

    private void LogLayers(GameObject root)
    {
        Debug.LogWarning($"PLAYER ROOT: {root.name} | layer={LayerMask.LayerToName(root.layer)} ({root.layer})");
        foreach (Transform child in root.transform)
            Debug.LogWarning($" └─ CHILD: {child.name} | layer={LayerMask.LayerToName(child.gameObject.layer)} ({child.gameObject.layer})");
    }
/// ///////////////////////////////////////////////////////////////
/*
    private void Awake()
    {
        lowLayer  = LayerMask.NameToLayer("Player_Low");
        highLayer = LayerMask.NameToLayer("Player_High");
    }

      void SetLayerRecursively(GameObject obj, int newLayer)
        {
            obj.layer = newLayer;

            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }

    private void OnTriggerEnter2D(Collider2D other)
    {

        //change layer for player's collision
        if (other.gameObject.layer == lowLayer)
        {
            var playerRoot = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject;
            SetLayerRecursively(playerRoot, highLayer);

            SetIgnorePlayerVsHighland(playerRoot, highlandTilemapGO, true);
        }
        else if (other.gameObject.layer == highLayer)
        {
            var playerRoot = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject;
            SetLayerRecursively(playerRoot, lowLayer);

            SetIgnorePlayerVsHighland(playerRoot, lowlandTilemapGO, true);
        }


        //playerRoot.GetComponent<PlayerGroundState>()?.SetHighland(true);
        Debug.LogWarning("Ignoring player vs ALL highland colliders on that tilemap GO");
        LogLayers(other.gameObject);
    }
    private void OnTriggerExit2D(Collider2D other)
    {

        if (other.gameObject.layer == highLayer)
        {
          var playerRoot = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject;
            SetIgnorePlayerVsHighland(playerRoot, highlandTilemapGO, false);
        }
        else if (other.gameObject.layer == lowLayer)
        {
            var playerRoot = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject;
            SetIgnorePlayerVsHighland(playerRoot, lowlandTilemapGO, false);
        }


        //playerRoot.GetComponent<PlayerGroundState>()?.SetHighland(false);

        Debug.LogWarning("Restored collision player vs highland");

    }

    void SetIgnorePlayerVsHighland(GameObject playerRoot, GameObject highlandObject, bool ignore)
    {
        var playerCols = playerRoot.GetComponentsInChildren<Collider2D>(includeInactive: true);
        var highlandCols = highlandObject.GetComponents<Collider2D>(); // grabs Composite + Tilemap + Edge, etc.

        foreach (var pc in playerCols)
        foreach (var hc in highlandCols)
            Physics2D.IgnoreCollision(pc, hc, ignore);
    }

    void LogLayers(GameObject root)
    {
        Debug.LogWarning(
            $"PLAYER ROOT: {root.name} | layer = {LayerMask.LayerToName(root.layer)} ({root.layer})"
        );

        foreach (Transform child in root.transform)
        {
            Debug.LogWarning(
                $" └─ CHILD: {child.name} | layer = {LayerMask.LayerToName(child.gameObject.layer)} ({child.gameObject.layer})"
            );
        }
    }
    */
    /*
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject player = other.gameObject;

        if (player.layer == LayerMask.NameToLayer("Player_Low"))
        {
            player.layer = highLayer;
            foreach (Transform child in player.transform)
            {
                child.gameObject.layer = highLayer;
                Debug.LogWarning("child of player new Mask:" + child.gameObject.layer);
            }
            //other.gameObject.layer = LayerMask.NameToLayer("Player_High");
            //Debug.Log("Entered and given new layer:" + other.gameObject.layer);
            highlandCollider.enabled = false;
            Debug.LogWarning($"Disabled: {highlandCollider.GetType().Name} on {highlandCollider.gameObject.name}, enabled={highlandCollider.enabled}");
        }

        else if (player.layer == LayerMask.NameToLayer("Player_High"))
        {
            player.layer = lowLayer;
            foreach (Transform child in player.transform)
            {
                child.gameObject.layer = lowLayer;
            }
            //other.gameObject.layer = LayerMask.NameToLayer("Player_Low");
            lowlandCollider.enabled = false;
        }
    }

   private void OnTriggerExit2D(Collider2D other)
    {
        GameObject player = other.gameObject;

        if (player.layer == LayerMask.NameToLayer("Player_High"))
        {
            highlandCollider.enabled = true;
        }

        else if (player.layer == LayerMask.NameToLayer("Player_Low"))
        {
            lowlandCollider.enabled = true;
        }
    }

*/
}
