using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class Elevation_Exit : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerElevationController elevation = other.GetComponentInParent<PlayerElevationController>();
        if (elevation != null)
        {
            elevation.SetLowGround();
        }
    }
}
