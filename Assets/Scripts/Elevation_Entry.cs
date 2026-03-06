using UnityEngine;

public class Elevation_Entry : MonoBehaviour
{
    [SerializeField] private Collider2D[] mountainColliders;
    [SerializeField] private Collider2D[] boundryColliders;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        { 
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
            sr.sortingLayerName = "High";
            sr.sortingOrder = 2;
        }

    }
}
