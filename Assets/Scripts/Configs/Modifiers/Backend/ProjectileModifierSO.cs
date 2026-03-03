using UnityEngine;
using UnityEngine.VFX;

public abstract class ProjectileModifierSO : ScriptableObject
{
    // Visual tweak at spawn time
    public virtual ProjectileConfigSO ModifyConfig(ProjectileConfigSO current) => null;
    
    public virtual ObjectPool ModifyPool(ObjectPool current) => null;

    // Called on hit (only when target is confirmed Enemy)
    public virtual void OnHitEnemy(GameObject attacker, GameObject enemy, Vector3 hitPos, Quaternion hitRot) { }
}
