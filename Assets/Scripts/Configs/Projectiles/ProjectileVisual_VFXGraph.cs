using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ProjectileVisual_VFXGraph : MonoBehaviour, IProjectileVisualReciever
{
    [SerializeField] private VisualEffect vfx;
    [Header("VFX property names")]
    [SerializeField] private string colorProp = "ElementColor";
    [SerializeField] private string freezeProp = "FreezeAmount";

    private void Reset() => vfx = GetComponentInChildren<VisualEffect>();

    public void Apply(IReadOnlyList<ProjectileModifierSO> mods, MaterialPropertyBlock mpb)
    {
        if (vfx == null || mods == null) return;

        // Example: let Freeze modifier set these via a method, or detect by type.
        // Best: each modifier exposes “visual params” (color/strength) rather than bullet IDs.
        for (int i = 0; i < mods.Count; i++)
        {
            if (mods[i] is TargetMovementModifierSO freeze) // if you have a Freeze-specific SO type
            {
                //vfx.SetVector4(colorProp, freeze.);
                //vfx.SetFloat(freezeProp, freeze.Strength);
            }
        }
    }
}
