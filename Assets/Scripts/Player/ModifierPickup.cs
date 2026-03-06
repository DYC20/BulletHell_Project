using UnityEngine;

public class ModifierPickup : MonoBehaviour, IPickup
{
    [SerializeField] private ModifierSO modifierConfig;
    [SerializeField] private bool destroyAfterPickup = true;

    public bool CanPickup(GameObject picker)
    {
        if (picker == null || modifierConfig == null) return false;

        var root = picker.transform.root;

        // modifier set is usually on player root or under it
        var set = root.GetComponentInChildren<ProjectileModifierSet>(true);
        if (set == null) return false;

        // weapon is usually a child (WeaponPivot/Weapon), so search children
        var weapon = root.GetComponentInChildren<IWeaponProjectileBase>(true);
        if (weapon == null || weapon.BaseConfig == null) return false;

        return true;
    }

    public void Pickup(GameObject picker)
    {
        if (picker == null || modifierConfig == null) return;

        var root = picker.transform.root;

        var set = root.GetComponentInChildren<ProjectileModifierSet>(true);
        if (set == null) return;

        var weapon = root.GetComponentInChildren<IWeaponProjectileBase>(true);
        if (weapon == null || weapon.BaseConfig == null) return;

        AmmoType currentAmmo = weapon.BaseConfig.ammoType;
        set.SetModifierFor(currentAmmo, modifierConfig);

        Debug.Log($"Picked {modifierConfig.name} for ammo={currentAmmo}. Current mod = {set.GetModifierFor(currentAmmo)?.name}");

        if (destroyAfterPickup)
            Destroy(gameObject);
    }
}