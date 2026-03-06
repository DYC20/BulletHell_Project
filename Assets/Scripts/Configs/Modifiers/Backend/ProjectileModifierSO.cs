using UnityEngine;
using UnityEngine.VFX;

public abstract class ProjectileModifierSO : ScriptableObject
{
    // Visual tweak at spawn time
    public virtual void Modify(ref ProjectileConfigSO config, ref ObjectPool pool) { }

    // Called on hit (only when target is confirmed Enemy)
    public virtual void OnHitEnemy(GameObject attacker, GameObject enemy, Vector3 hitPos, Quaternion hitRot) { }
}
