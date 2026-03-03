using UnityEngine;
using System.Collections.Generic;

public class ProjectileModifierSet : MonoBehaviour
{
    [SerializeField] private List<ProjectileModifierSO> active = new();

    public IReadOnlyList<ProjectileModifierSO> Active => active;
    public bool HasModifier(ProjectileModifierSO mod) => mod != null && active.Contains(mod);
    
    public void AddModifier(ProjectileModifierSO mod)
    {
        if (mod != null && !active.Contains(mod))
            active.Add(mod);
    }

    public void RemoveModifier(ProjectileModifierSO mod)
    {
        if (mod != null)
            active.Remove(mod);
        
        var state = GetComponentInParent<ModifierRuntimeState>();
        if (state != null) state.ClearModifier(mod);
    }
    /// Call this when selecting the config to shoot with.
    public ProjectileConfigSO GetModifiedConfig(ProjectileConfigSO baseConfig)
    {
        ProjectileConfigSO current = baseConfig;

        for (int i = 0; i < active.Count; i++)
        {
            var ov = active[i].ModifyConfig(current);
            if (ov != null) current = ov;
        }

        return current;
    }

    public ObjectPool GetModifiedPool(ObjectPool basePool)
    {
        ObjectPool current = basePool;
        for (int i = 0; i < active.Count; i++)
        {
            var ov = active[i].ModifyPool(current);
            if (ov != null) current = ov;
        }
        return current;
    }
    
    /// Call this from projectile hit logic (only after Enemy tag check).
    public void NotifyHitEnemy(GameObject attacker, GameObject enemy, Vector3 hitPos, Quaternion hitRot)
    {
        for (int i = 0; i < active.Count; i++)
            active[i].OnHitEnemy(attacker, enemy, hitPos, hitRot);
    }
}
