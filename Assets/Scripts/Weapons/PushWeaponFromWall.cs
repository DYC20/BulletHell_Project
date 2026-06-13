using UnityEngine;

public class PushWeaponFromWall : MonoBehaviour
{
    private Collider _collider;

    private void Start()
    {
        _collider = GetComponent<Collider>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Walls"))
        {
            
        }
    }
    
}
