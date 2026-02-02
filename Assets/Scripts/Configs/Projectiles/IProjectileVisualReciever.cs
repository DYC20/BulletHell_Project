using System.Collections.Generic;
using UnityEngine;

public interface IProjectileVisualReciever
{
    // Called on Init, after stats are modified
    void Apply(IReadOnlyList<ProjectileModifierSO> mods, MaterialPropertyBlock mpb);
}
