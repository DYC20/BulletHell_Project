using UnityEngine;
using UnityEngine.VFX;

[CreateAssetMenu(menuName = "Upgrades/Projectile Modifiers/Freeze")]
public class TargetMovementModifierSO : ProjectileModifierSO
{
    [SerializeField, ColorUsage(true, true)] private Color tint ;
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private float slowDuration = 1.5f;
    [SerializeField] private float slowFactor = 0.5f;

    public override void ApplyVisuals( MaterialPropertyBlock mpb)
    {
        mpb.SetColor("_Color", tint);
        Debug.LogWarning("FreezeModifierSO: ApplyVisuals called");
    }

    public override void OnHit(GameObject target, GameObject instigator)
    {
        var enemyAI = target.GetComponentInParent<EnemyChaseAI>(); // your status system
        if (enemyAI != null)
        {
             enemyAI.MoveSpeed *= slowFactor;
             //slow.ApplySlow(slowFactor, slowDuration);
             Transform hitLocation = enemyAI.transform;
             Instantiate(hitEffect, hitLocation.position, hitLocation.rotation);
        }
           
    }


}