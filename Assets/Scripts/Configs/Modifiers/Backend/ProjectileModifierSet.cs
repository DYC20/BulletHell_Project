using UnityEngine;
using System.Collections.Generic;

public class ProjectileModifierSet : MonoBehaviour
{
    [SerializeField] private List<AmmoType> debugKeys = new();

    private readonly Dictionary<AmmoType, ProjectileModifierSO> _perAmmo = new();

    public ProjectileModifierSO GetModifierFor(AmmoType ammoType)
        => _perAmmo.TryGetValue(ammoType, out var mod) ? mod : null;

    /// Adds or REPLACES the modifier for this ammo type (no stacking).
    public void SetModifierFor(AmmoType ammoType, ProjectileModifierSO mod)
    {
        if (mod == null) return;
        _perAmmo[ammoType] = mod;
        RefreshDebugKeys();
    }

    public void ClearModifierFor(AmmoType ammoType)
    {
        _perAmmo.Remove(ammoType);
        RefreshDebugKeys();
    }
    public void ApplyForCurrentAmmo(AmmoType ammoType, ref ProjectileConfigSO config, ref ObjectPool pool)
    {
        if (_perAmmo.TryGetValue(ammoType, out var mod) && mod != null)
            mod.Modify(ref config, ref pool);
    }

    public void NotifyHitEnemy(AmmoType ammoType, GameObject attacker, GameObject enemy, Vector3 hitPos, Quaternion hitRot)
    {
        if (_perAmmo.TryGetValue(ammoType, out var mod) && mod != null)
            mod.OnHitEnemy(attacker, enemy, hitPos, hitRot);
    }
    private void RefreshDebugKeys()
    {
        debugKeys.Clear();
        foreach (var k in _perAmmo.Keys) debugKeys.Add(k);
    }
}
