using UnityEngine;
using UnityEngine.VFX;

public class ProjectileShootEffect : MonoBehaviour
{
    public void Apply(VisualEffect shootEffect, Vector3 position, Quaternion rotation)
    {
        Instantiate(shootEffect, transform.position, rotation);
        Debug.Log("ShootEffect Instantiated:" + shootEffect.name + "position" + position + "rotation" + rotation);
    }
}
