using UnityEngine;

public class PlayerRendererGetter : MonoBehaviour
{
   [SerializeField] SpriteRenderer spriteRenderer;
   private string currentSortingLayerName;
   private int currentSortingOrder;

   public string GetRenderLayer => currentSortingLayerName;
   public int GetSortingOrder => currentSortingOrder;

   private void Start()
   {
      currentSortingLayerName = spriteRenderer.sortingLayerName;
      currentSortingOrder = spriteRenderer.sortingOrder;
   }

   public void SetRenderLayer(string layerName)
   {
      currentSortingLayerName = layerName;
   }
}
