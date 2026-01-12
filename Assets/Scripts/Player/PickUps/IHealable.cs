using UnityEngine;

public interface IHealable
{
    bool CanHeal(float amount);
    void Heal(float amount);
}