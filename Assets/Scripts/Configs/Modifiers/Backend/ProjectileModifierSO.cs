using UnityEngine;
using UnityEngine.VFX;

public abstract class ProjectileModifierSO : ScriptableObject
{
    public virtual void ModifyStats(ref ProjectileStats stats) { }

    // Visual tweak at spawn time
    public virtual void ApplyVisuals(MaterialPropertyBlock mpb) { }

    // Optional: apply a status effect on hit
    public virtual void OnHit(GameObject target, GameObject instigator) { }
}
