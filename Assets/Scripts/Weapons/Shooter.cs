using UnityEngine;
/*
public class Shooter : MonoBehaviour
{
    [SerializeField] private BulletPool bulletPool;
    [SerializeField] private Transform firePoint;

    public void Shoot()
    {
        var bullet = bulletPool.Get(firePoint.position, firePoint.rotation);

        // owner (prevents self hit)
        var dmg = bullet.GetComponent<BulletDamage>();
        if (dmg) dmg.owner = gameObject;

        // fire forward (use up for top-down; use right if your sprite faces right)
        var move = bullet.GetComponent<BulletMovement>();
        if (move) move.Fire(firePoint.up);
    }
}
*/