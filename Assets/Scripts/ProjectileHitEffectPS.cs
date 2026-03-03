using UnityEngine;

public class ProjectileHitEffectPS : MonoBehaviour
    {
        public void Apply(ParticleSystem hitEffectPS, Vector3 position, Quaternion rotation)
        {
            Instantiate(hitEffectPS, transform.position, rotation);
//        Debug.Log("ShootEffect Instantiated:" + shootEffect.name + "position" + position + "rotation" + rotation);
        }
    }
