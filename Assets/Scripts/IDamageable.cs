using UnityEngine;

public interface IDamageable
{
    Teams Team { get; }
    void TakeDamage(float amount, GameObject instigator);
}