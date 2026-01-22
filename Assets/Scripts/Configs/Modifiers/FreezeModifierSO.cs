using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Projectile Modifiers/Freeze")]
public class FreezeModifierSO : ProjectileModifierSO
{
    [SerializeField, ColorUsage(true, true)] private Color tint ;
    [SerializeField] private float slowDuration = 1.5f;
    [SerializeField] private float slowFactor = 0.5f;

    public override void ApplyVisuals( MaterialPropertyBlock mpb)
    {
        mpb.SetColor("_Color", tint);
        Debug.LogWarning("FreezeModifierSO: ApplyVisuals called");
    }

    public override void OnHit(GameObject target, GameObject instigator)
    {
        var slow = target.GetComponentInParent<EnemyChaseAI>(); // your status system
        if (slow != null)
            slow.moveSpeed *= slowFactor;
            //slow.ApplySlow(slowFactor, slowDuration);
    }
}