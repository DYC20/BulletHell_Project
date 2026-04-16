using UnityEngine;
using UnityEngine.InputSystem;

public class Elevation_Entry : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerElevationController elevation = other.GetComponentInParent<PlayerElevationController>();
        if (elevation != null)
        {
            elevation.SetHighGround();
        }
    }
}
