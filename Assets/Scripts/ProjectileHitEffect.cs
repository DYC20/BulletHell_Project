using UnityEngine;
using UnityEngine.VFX;

public class ProjectileHitEffect : MonoBehaviour
{
    public void Apply(VisualEffect hitEffect, Vector3 position, Quaternion rotation)
    {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
            Debug.Log("HitEffect Instantiated:" + hitEffect.name + "position" + position + "rotation" + rotation);
    }
}
