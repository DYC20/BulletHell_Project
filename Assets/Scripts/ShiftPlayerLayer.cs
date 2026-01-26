using System.Collections.Generic;
using UnityEngine;

public class ShiftPlayerLayer : MonoBehaviour
{
    [Header("Only react to colliders with this tag")]
    [SerializeField] private string requiredTag = "GroundCollider";

    [Header("Tilemap GameObjects that hold the colliders to ignore/restore")]
    [SerializeField] private GameObject highlandTilemapGO;
    [SerializeField] private GameObject lowlandTilemapGO;

    // Track how many qualifying colliders of a given playerRoot are inside this trigger
    private readonly Dictionary<GameObject, int> _insideCount = new Dictionary<GameObject, int>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only act for the designated collider(s)
        if (!other.CompareTag(requiredTag))
            return;

        var playerRoot = GetPlayerRoot(other);
        if (playerRoot == null) return;

        // Increase overlap count
        if (_insideCount.TryGetValue(playerRoot, out int count))
            _insideCount[playerRoot] = count + 1;
        else
            _insideCount[playerRoot] = 1;

        // Only act on the FIRST enter (prevents double calls from multiple colliders)
        if (_insideCount[playerRoot] != 1)
            return;

        // On enter: ignore BOTH highland and lowland
        SetIgnorePlayerVsTilemap(playerRoot, highlandTilemapGO, true);
        SetIgnorePlayerVsTilemap(playerRoot, lowlandTilemapGO, true);

        Debug.LogWarning($"ShiftPlayerLayer ENTER: Ignoring HIGH+LOW collisions | root={playerRoot.name}");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(requiredTag))
            return;

        var playerRoot = GetPlayerRoot(other);
        if (playerRoot == null) return;

        if (!_insideCount.TryGetValue(playerRoot, out int count))
            return;

        count--;

        if (count <= 0)
        {
            _insideCount.Remove(playerRoot);

            var autoAssign = playerRoot.GetComponent<GroundLayerAutoAssign>();
            if (autoAssign != null)
            {
                int evaluated = autoAssign.EvaluateAndApplyLayer();
                Debug.Log($"ShiftPlayerLayer EXIT: EvaluateLayerOnly => {LayerMask.LayerToName(evaluated)} ({evaluated})");
            }
            else
            {
                Debug.LogWarning($"ShiftPlayerLayer EXIT: {playerRoot.name} has no GroundLayerAutoAssign.");
            }
            
            // On exit (last collider left): restore BOTH
            SetIgnorePlayerVsTilemap(playerRoot, highlandTilemapGO, false);
            SetIgnorePlayerVsTilemap(playerRoot, lowlandTilemapGO, false);

            Debug.LogWarning($"ShiftPlayerLayer EXIT: Restored HIGH+LOW collisions | root={playerRoot.name}");
        }
        else
        {
            _insideCount[playerRoot] = count;
        }
    }

    // -------- Helpers --------

    private GameObject GetPlayerRoot(Collider2D col)
    {
        // Expected setup: Rigidbody2D on player root, colliders on children
        if (col.attachedRigidbody != null)
            return col.attachedRigidbody.gameObject;

        // Fallback
        return col.transform.root.gameObject;
    }

    private void SetIgnorePlayerVsTilemap(GameObject playerRoot, GameObject tilemapGO, bool ignore)
    {
        if (playerRoot == null || tilemapGO == null) return;

        var playerCols = playerRoot.GetComponentsInChildren<Collider2D>(includeInactive: true);
        var tilemapCols = tilemapGO.GetComponents<Collider2D>(); // Composite/Tilemap/Edge etc.

        foreach (var pc in playerCols)
        foreach (var tc in tilemapCols)
            Physics2D.IgnoreCollision(pc, tc, ignore);
    }
}