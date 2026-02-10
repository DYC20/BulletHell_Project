using UnityEngine;

public class ProjectileShootEffectPS : MonoBehaviour

{
    public void Apply(ParticleSystem shootEffectPS, Vector3 position, Quaternion rotation)
    {
        Instantiate(shootEffectPS, transform.position, rotation);
//        Debug.Log("ShootEffect Instantiated:" + shootEffect.name + "position" + position + "rotation" + rotation);
    }
}
