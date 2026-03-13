using UnityEngine;
using UnityEngine.InputSystem;

public class Elevation_Entry : MonoBehaviour
{
    [SerializeField] private Collider2D[] mountainColliders;
    [SerializeField] private Collider2D[] boundryColliders;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        { 
            PlayerController controller = other.GetComponentInParent<PlayerController>();
            
            if (controller != null)
                controller.SetPlayerGrounded(false);
            
            foreach (Collider2D _mountain in mountainColliders)
            {
                _mountain.enabled = false;
                
            } 
            foreach (Collider2D _Boundry in boundryColliders)
            {
                _Boundry.enabled = true;
            }
        }
        foreach (SpriteRenderer sr in other.GetComponentsInChildren<SpriteRenderer>(true))
        {
            sr.sortingLayerName = "HighGround";
            sr.sortingOrder = 2;
        }

    }
}
