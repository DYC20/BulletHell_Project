using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Projectile Modifiers")]
public class ModifierSO : ProjectileModifierSO
{
    public override void OnHit(GameObject target, GameObject instigator)
    {
        var enemyAI = target.GetComponentInParent<EnemyChaseAI>(); // your status system
        if (enemyAI != null)
        {
            //enemyAI.MoveSpeed *= slowFactor;
            //slow.ApplySlow(slowFactor, slowDuration);
            Transform hitLocation = enemyAI.transform;
            // Instantiate(hitEffect, hitLocation.position, hitLocation.rotation);
        }

    }
}
