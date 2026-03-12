using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.VFX;

public class ModifierRuntimeState : MonoBehaviour
{
    // modifier -> enemy -> hit count
    private readonly Dictionary<int, Dictionary<int, int>> _hitCounts = new();
    
    [Header("UpdateUI")] 
    [SerializeField] public Image weaponBG;
    [SerializeField] public VisualEffect fireUIEffect;
    [SerializeField] public VisualEffect iceUIEffect;
    [SerializeField] public Color fireNewColor;
    [SerializeField] public Color iceNewColor;
    [SerializeField] public float newColorDuration;
    public static ModifierRuntimeState Instance { get; private set; }

    public bool isIce;
    [HideInInspector]
    public bool isModified;
    
    private class Snapshot
    {
        public bool hasMove;
        public float move;

        public bool hasFire;
        public float fireInterval;

        public Coroutine revertRoutine;
        public Coroutine damageRoutine;
    }

    // modifier -> enemy -> snapshot
    private readonly Dictionary<int, Dictionary<int, Snapshot>> _snapshots = new();

    private void Awake()
    {
        Instance = this;
    }

    public void SetIce(bool value)
    {
        isIce = value;
    }
    public void SetModifiedState(bool value)
    {
        isModified = value;
    }
    
    public int IncrementHit(ScriptableObject modifier, GameObject enemy)
    {
        if (modifier == null || enemy == null) return 0;

        int modKey = modifier.GetInstanceID();
        int enemyKey = enemy.GetInstanceID();

        if (!_hitCounts.TryGetValue(modKey, out var perEnemy))
        {
            perEnemy = new Dictionary<int, int>();
            _hitCounts.Add(modKey, perEnemy);
        }

        perEnemy.TryGetValue(enemyKey, out int count);
        count++;
        perEnemy[enemyKey] = count;
        return count;
    }

    public void ClearModifier(ScriptableObject modifier)
    {
        if (modifier == null) return;

        int modKey = modifier.GetInstanceID();

        _hitCounts.Remove(modKey);

        if (_snapshots.TryGetValue(modKey, out var perEnemy))
        {
            foreach (var kv in perEnemy)
            {
                if (kv.Value.revertRoutine != null)
                    StopCoroutine(kv.Value.revertRoutine);
            }
        }

        _snapshots.Remove(modKey);
    }

    /// Applies timed debuff using baseline snapshot (per modifier+enemy).
    /// If already active, it refreshes the timer but keeps the original baseline.
    public void ApplyTimedDebuff(
        ScriptableObject modifier,
        GameObject enemy,
        GameObject damageFX,
        float moveSpeedMul,
        float fireIntervalMul,
        float durationSeconds, 
        float damage
    )
    {
        if (modifier == null || enemy == null) return;

        int modKey = modifier.GetInstanceID();
        int enemyKey = enemy.GetInstanceID();

        if (!_snapshots.TryGetValue(modKey, out var perEnemy))
        {
            perEnemy = new Dictionary<int, Snapshot>();
            _snapshots.Add(modKey, perEnemy);
        }

        if (!perEnemy.TryGetValue(enemyKey, out var snap))
        {
            snap = new Snapshot();

            var move = enemy.GetComponentInParent<IEnemyMoveSpeed>();
            if (move != null)
            {
                snap.hasMove = true;
                snap.move = move.MoveSpeed;
            }

            var fire = enemy.GetComponentInParent<IEnemyFireInterval>();
            if (fire != null)
            {
                snap.hasFire = true;
                snap.fireInterval = fire.FireInterval;
            }

            perEnemy.Add(enemyKey, snap);
        }

        // refresh timer
        if (snap.revertRoutine != null)
            StopCoroutine(snap.revertRoutine);

        // apply modified values (only supported capabilities)
        var m = enemy.GetComponentInParent<IEnemyMoveSpeed>();
        if (snap.hasMove && m != null) m.MoveSpeed = snap.move * moveSpeedMul;

        var f = enemy.GetComponentInParent<IEnemyFireInterval>();
        if (snap.hasFire && f != null) f.FireInterval = snap.fireInterval * fireIntervalMul;

        var d = enemy.GetComponentInParent<IDamageable>();
        if (d == null)
        {
            Debug.Log("IDamageable is null");
        }
        if (d != null && durationSeconds > 0 && damage > 0)
        {
            if (snap.damageRoutine != null)
                StopCoroutine(snap.damageRoutine);

            snap.damageRoutine = StartCoroutine(DamageOverTime(enemy, damage,damageFX, durationSeconds, modifier));
        }
            
        

        snap.revertRoutine = StartCoroutine(RevertAfter(modKey, enemyKey, enemy, durationSeconds));
    }
    

    private IEnumerator RevertAfter(int modKey, int enemyKey, GameObject enemy, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (enemy == null) yield break;

        if (!_snapshots.TryGetValue(modKey, out var perEnemy)) yield break;
        if (!perEnemy.TryGetValue(enemyKey, out var snap)) yield break;

        var m = enemy.GetComponentInParent<IEnemyMoveSpeed>();
        if (snap.hasMove && m != null) m.MoveSpeed = snap.move;

        var f = enemy.GetComponentInParent<IEnemyFireInterval>();
        if (snap.hasFire && f != null) f.FireInterval = snap.fireInterval;
        
        if (snap.damageRoutine != null)
            StopCoroutine(snap.damageRoutine);

        perEnemy.Remove(enemyKey);
        if (perEnemy.Count == 0) _snapshots.Remove(modKey);
    }

    private IEnumerator DamageOverTime(GameObject enemy, float damagePerSecond,GameObject damageFX, float duration, ScriptableObject modifier)
    {
        float timer = 0f;

        while (timer < duration)
        {
            if (enemy == null) yield break;

            var d = enemy.GetComponentInParent<IDamageable>();

            if (d != null)
            {
                d.TakeDamage(damagePerSecond, null);
                Transform enemyVisualMiddle = enemy.transform.Find("EnemyVisualMiddle");
                Instantiate(damageFX, enemyVisualMiddle.transform.position, Quaternion.identity);
                Debug.Log($"Damage From Modifier: {modifier.name}, Amount: {damagePerSecond}");
            }

            yield return new WaitForSeconds(1f);

            timer += 1f;
        }
    }
}