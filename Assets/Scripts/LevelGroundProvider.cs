using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGroundProvider : MonoBehaviour
{
    public static LevelGroundProvider Instance { get; private set; }

    [Header("Ground tilemaps (tile data only)")]
    public Tilemap highGround;
    public Tilemap lowGround;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}

