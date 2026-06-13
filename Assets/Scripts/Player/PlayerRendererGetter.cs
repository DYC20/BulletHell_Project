using UnityEngine;

public class PlayerRendererGetter : MonoBehaviour
{
   [SerializeField] SpriteRenderer spriteRenderer;
   private string currentSortingLayerName;

   public string GetRenderLayer => currentSortingLayerName;

   private void Start()
   {
      currentSortingLayerName = spriteRenderer.sortingLayerName;
   }

   public void SetRenderLayer(string layerName)
   {
      currentSortingLayerName = layerName;
   }
}
