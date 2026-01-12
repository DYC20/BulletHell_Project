using UnityEngine;

public interface IAmmoConsumer
{
    bool HasAmmo(AmmoType type, int amount);
    bool TryConsumeAmmo(AmmoType type, int amount);
}
