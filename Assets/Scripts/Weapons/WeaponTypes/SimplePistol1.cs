using UnityEngine;
using Unity.Cinemachine;
/*
public class SniperRifle_01 : WeaponBase
{
    
    [Header("Camera Shake")]
    [SerializeField] private float recoilStrength = 0.2f;
    [SerializeField] private CinemachineImpulseSource recoilImpulse;
    
    protected override void FireInternal()
    {
        if (bulletPool == null)
        {
            Debug.LogError($"{name}: bulletPool is NULL");
            return;
        }

        var bullet = bulletPool.Get(firePoint.position, firePoint.rotation);

        var dmg = bullet.GetComponent<projectileDamage>();
        if (dmg == null)
        {
            Debug.LogError("Bullet has no BulletDamage component!");
            return;
        }

        dmg.owner = owner;

        var move = bullet.GetComponent<BulletMovement>();
        if (move != null)
            move.Fire(firePoint.up);
        Vector3 fireDirection = firePoint.up;


        if (recoilImpulse != null)
            recoilImpulse.GenerateImpulse(recoilStrength);
                recoilImpulse.DefaultVelocity = fireDirection;
            
    }
    
}
*/