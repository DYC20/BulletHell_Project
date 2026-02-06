using UnityEngine;

public class Elevation_Exit : MonoBehaviour
{
    public Collider2D[] mountainColliders;
    public Collider2D[] boundryColliders;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        { 
            foreach (Collider2D _mountain in mountainColliders)
            {
                _mountain.enabled = true;
                Debug.Log($"{_mountain.name} → collider enabled");
                
            } 
            foreach (Collider2D _Boundry in boundryColliders)
            {
                _Boundry.enabled = false;
            }
        }

        other.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 1;

    }
}
