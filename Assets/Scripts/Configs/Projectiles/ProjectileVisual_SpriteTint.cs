using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ProjectileVisual_SpriteTint : MonoBehaviour, IProjectileVisualReciever
{
    [SerializeField] private SpriteRenderer sr;

    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private void Reset() => sr = GetComponent<SpriteRenderer>();

    public void Apply(IReadOnlyList<ProjectileModifierSO> mods, MaterialPropertyBlock mpb)
    {
        if (sr == null) return;

        sr.GetPropertyBlock(mpb);
        mpb.Clear();

        // IMPORTANT: don’t use sr.material.color (creates material instances).
        // Use sharedMaterial to read a "base" color, or just define it in inspector.
        var baseColor = sr.sharedMaterial != null ? sr.sharedMaterial.color : Color.white;

        mpb.SetColor(ColorId, baseColor);

        if (mods != null)
        {
            for (int i = 0; i < mods.Count; i++)
                mods[i].ApplyVisuals(mpb);
        }

        sr.SetPropertyBlock(mpb);
    }
}
