using UnityEngine;

public interface IAmmoReceiver
{
    bool CanReceiveAmmo(AmmoType type, int amount);
    void ReceiveAmmo(AmmoType type, int amount);
}