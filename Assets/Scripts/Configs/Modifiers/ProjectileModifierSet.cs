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
    }
}
