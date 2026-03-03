using UnityEngine;
using UnityEngine.VFX;

[CreateAssetMenu(menuName = "Upgrades/Projectile Modifiers")]
public class ModifierSO : ProjectileModifierSO
{
    [Header("Config Override")]
    [SerializeField] private ProjectileConfigSO projectileConfig;
    
    [Header("Pool Override")]
    [SerializeField] private ObjectPool poolOverride;

    [Header("Hit Counting")]
    [SerializeField] private int hitsToTrigger = 3;

    [Header("Timed Enemy Debuff")]
    [SerializeField] private float debuffDuration = 3f;

    // Multipliers: 1 = no change, 0.7 = slower, 1.3 = faster, etc.
    [SerializeField] private float moveSpeedMultiplier = 0.7f;
    [SerializeField] private float fireIntervalMultiplier = 1.4f; // higher interval => shoots slower

    [Header("FX Prefab (ParticleSystem or VFX Graph)")]
    [SerializeField] private GameObject fullEffectPrefab;

    public override ProjectileConfigSO ModifyConfig(ProjectileConfigSO current)
    {
        return projectileConfig != null ? projectileConfig : null;
    }
    public override ObjectPool ModifyPool(ObjectPool current)
        => poolOverride != null ? poolOverride : null;

    public override void OnHitEnemy(GameObject attacker, GameObject enemy, Vector3 hitPos, Quaternion hitRot)
    {
        if (attacker == null || enemy == null) return;

        var state = attacker.GetComponentInParent<ModifierRuntimeState>();
        if (state == null) return;

        int count = state.IncrementHit(this, enemy);

        if (count == hitsToTrigger)
        {
            // apply timed behavior changes (reverts automatically)
            state.ApplyTimedDebuff(
                modifier: this,
                enemy: enemy,
                moveSpeedMul: moveSpeedMultiplier,
                fireIntervalMul: fireIntervalMultiplier,
                durationSeconds: debuffDuration
            );

            // spawn FX on enemy and auto-destroy when done
            if (fullEffectPrefab != null)
            {
                var fx = Object.Instantiate(fullEffectPrefab, enemy.transform.position, hitRot, enemy.transform);
            }
        }
    }
}
