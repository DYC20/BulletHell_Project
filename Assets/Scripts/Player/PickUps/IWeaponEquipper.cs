using UnityEngine;

public interface IWeaponEquipper
{
    bool CanEquip(GameObject weaponPrefab);
    void Equip(GameObject weaponPrefab);
}
