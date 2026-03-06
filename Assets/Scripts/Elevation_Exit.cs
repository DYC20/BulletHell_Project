using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class Elevation_Exit : MonoBehaviour
{
    public Collider2D[] mountainColliders;
    public Collider2D[] boundryColliders;
    
    IEnumerator CheckNextFrame(Collider2D c)
    {
        WaitForSeconds wait = new WaitForSeconds(1f);
        yield return wait; // wait 1 frame
        Debug.Log($"NEXT FRAME -> {c.name} enabled:{c.enabled}", c.gameObject);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        { 
            foreach (Collider2D _mountain in mountainColliders)
            {
                _mountain.enabled = true;
                Debug.Log(
                    $"ENABLED -> {_mountain.name} | enabled:{_mountain.enabled} | activeInHierarchy:{_mountain.gameObject.activeInHierarchy} | instanceID:{_mountain.GetInstanceID()}",
                    _mountain.gameObject);
             
                StartCoroutine(CheckNextFrame(_mountain));
                
            } 
            foreach (Collider2D _Boundry in boundryColliders)
            {
                _Boundry.enabled = false;
            }
        
            foreach (SpriteRenderer sr in other.GetComponentsInChildren<SpriteRenderer>(true))
            {
              sr.sortingLayerName = "Player";
             sr.sortingOrder = 1;
            }
        //other.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 1;
        }
    }
}
