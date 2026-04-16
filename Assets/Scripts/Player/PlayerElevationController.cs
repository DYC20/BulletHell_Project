using UnityEngine;

public class PlayerElevationController : MonoBehaviour
{
    [Header("Layer names")]
    [SerializeField] private string lowPlayerLayer = "Player_Low";
    [SerializeField] private string highPlayerLayer = "Player_High";

    [Header("Visual sorting")]
    [SerializeField] private string lowSortingLayer = "Ground";
    [SerializeField] private int lowSortingOrder = 1;
    [SerializeField] private string highSortingLayer = "HighGround";
    [SerializeField] private int highSortingOrder = 2;

    private PlayerController playerController;
    private int lowLayerIndex;
    private int highLayerIndex;
    private bool isHighGround;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();

        lowLayerIndex = LayerMask.NameToLayer(lowPlayerLayer);
        highLayerIndex = LayerMask.NameToLayer(highPlayerLayer);

        if (lowLayerIndex == -1)
            Debug.LogError($"Layer '{lowPlayerLayer}' does not exist.");
        if (highLayerIndex == -1)
            Debug.LogError($"Layer '{highPlayerLayer}' does not exist.");
    }

    private void Start()
    {
        SetLowGround();
    }

    public void SetHighGround()
    {
        if (isHighGround)
            return;

        isHighGround = true;

        SetLayerRecursively(gameObject, highLayerIndex);

        if (playerController != null)
            playerController.SetPlayerGrounded(false);

        SetSorting(highSortingLayer, highSortingOrder);
    }

    public void SetLowGround()
    {
        if (!isHighGround && gameObject.layer == lowLayerIndex)
            return;

        isHighGround = false;

        SetLayerRecursively(gameObject, lowLayerIndex);

        if (playerController != null)
            playerController.SetPlayerGrounded(true);

        SetSorting(lowSortingLayer, lowSortingOrder);
    }

    private void SetSorting(string sortingLayerName, int sortingOrder)
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in renderers)
        {
            sr.sortingLayerName = sortingLayerName;
            sr.sortingOrder = sortingOrder;
        }
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}