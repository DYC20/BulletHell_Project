using UnityEngine;

public class Elevation_Entry : MonoBehaviour
{
    public Collider2D[] mountainColliders;
    public Collider2D[] boundryColliders;

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

        other.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 20;

    }
}
