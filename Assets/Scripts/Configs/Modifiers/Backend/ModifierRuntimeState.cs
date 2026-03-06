using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ModifierRuntimeState : MonoBehaviour
{
    // modifier -> enemy -> hit count
    private readonly Dictionary<int, Dictionary<int, int>> _hitCounts = new();

    private class Snapshot
    {
        public bool hasMove;
        public float move;

        public bool hasFire;
        public float fireInterval;

        public Coroutine revertRoutine;
    }

    // modifier -> enemy -> snapshot
    private readonly Dictionary<int, Dictionary<int, Snapshot>> _snapshots = new();

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
        float moveSpeedMul,
        float fireIntervalMul,
        float durationSeconds
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

        perEnemy.Remove(enemyKey);
        if (perEnemy.Count == 0) _snapshots.Remove(modKey);
    }
}