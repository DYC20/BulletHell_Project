using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.VFX;

[CreateAssetMenu(menuName = "Upgrades/Projectile Modifiers")]
public class ModifierSO : ProjectileModifierSO
{
    [System.Serializable]
    private struct OverrideSet
    {
        public ProjectileConfigSO config;
        public ObjectPool pool;
    }
    [Header("Override Sets (choose based on CURRENT config ammoType)")]
    
    [SerializeField] private OverrideSet bulletSet;
    [SerializeField] private OverrideSet shellSet;
    [SerializeField] private OverrideSet explosiveSet;
    //[SerializeField] private OverrideSet rifleSet;

    [Header("Hit Counting")]
    [SerializeField] private int hitsToTrigger = 3;

    [Header("Timed Enemy Debuff")]
    [SerializeField] private float debuffDuration = 3f;
    [SerializeField] private float moveSpeedMultiplier = 0.7f;
    [SerializeField] private float fireIntervalMultiplier = 1.4f; // higher interval => shoots slower
    [SerializeField] private float damagePerSconds = 0f;
    [SerializeField] private GameObject damageFX;
    [SerializeField] private bool isStatic;

    [Header("FX Prefab (ParticleSystem or VFX Graph)")]
    [SerializeField] private GameObject fullEffectPrefab;
    
    [Header("Electric Material")]
    public Material tempEnemyMaterial;


    public override void Modify(ref ProjectileConfigSO config, ref ObjectPool pool)
    {
        if (config == null) return;

        OverrideSet set = default;

        switch (config.ammoType)
        {
            case AmmoType.Bullet: set = bulletSet; break;
            case AmmoType.Shell:  set = shellSet;  break;
            case AmmoType.Explosives:  set = explosiveSet;  break;
           // case AmmoType.Rifle:  set = rifleSet;  break;
            default: return;
        }

        

        if (set.config != null) config = set.config;
        if (set.pool != null) pool = set.pool;
    }

    public void ApplyDebuffDirect(GameObject attacker, GameObject enemy, Quaternion hitRot)
    {
        if (attacker == null || enemy == null) return;

        var state = attacker.GetComponentInParent<ModifierRuntimeState>();
        if (state == null) return;

        state.ApplyTimedDebuff(
            modifier: this,
            enemy: enemy,
            damageFX: damageFX,
            moveSpeedMul: moveSpeedMultiplier,
            fireIntervalMul: fireIntervalMultiplier,
            durationSeconds: debuffDuration,
            damage: damagePerSconds,
            makeBodyStatic: isStatic,
            tempEnemyMaterial: tempEnemyMaterial
        );

        if (fullEffectPrefab != null)
        {
            Transform enemyVisualMiddle = enemy.transform.Find("VisualCenter(DNCN)");
            Vector3 fxPos = enemyVisualMiddle != null ? enemyVisualMiddle.position : enemy.transform.position;

            Object.Instantiate(fullEffectPrefab, fxPos, hitRot, enemy.transform);
        }
    }
    
    public override void OnHitEnemy(GameObject attacker, GameObject enemy, Vector3 hitPos, Quaternion hitRot, int shotid)
    {
        if (attacker == null || enemy == null) return;

        var state = attacker.GetComponentInParent<ModifierRuntimeState>();
        if (state == null) return;

        int count = state.IncrementHit(this, enemy);
        
        float hitFreezeDuration = state.HitFreezeDuration;
        float fullEffectDuration = state.FullEffectFreezeDuration;
        
        bool canFreezeThisShot = state.TryConsumeShotFreeze(shotid);

        if (canFreezeThisShot)
        {
            float freezeDuration = count == hitsToTrigger
                ? fullEffectDuration
                : hitFreezeDuration;

            state.StartCoroutine(state.FreezeFrame(freezeDuration));
        }
        
            
        if (count == hitsToTrigger)
        {
           // state.StartCoroutine(state.FreezeFrame(fullEffectDuration));
            var damageable = enemy.GetComponentInParent<IDamageable>();
            // apply timed behavior changes (reverts automatically)
            state.ApplyTimedDebuff(
                modifier: this,
                enemy: enemy,
                damageFX: damageFX,
                moveSpeedMul: moveSpeedMultiplier,
                fireIntervalMul: fireIntervalMultiplier,
                durationSeconds: debuffDuration,
                damage: damagePerSconds,
                makeBodyStatic: isStatic,
                tempEnemyMaterial: tempEnemyMaterial
            );

            // spawn FX on enemy and auto-destroy when done
            if (fullEffectPrefab != null)
            {
                //Transform enemyVisualMiddle = enemy.transform.Find("VisualCenter(DNCN)");
                
                Debug.Log("enemy object is: " + enemy.name);
                Debug.Log("enemy root is: " + enemy.transform.root.name);

                foreach (Transform child in enemy.transform)
                {
                    Debug.Log("Direct child of enemy: [" + child.name + "]");
                }
                
                if (enemy == null)
                {
                    Debug.LogWarning("EnemyVisualMiddle not found!");
                }
                var fx = Object.Instantiate(fullEffectPrefab, enemy.transform.position, hitRot, enemy.transform);
            }
        }
    }
    
}
