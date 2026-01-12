using UnityEngine;

public class WeaponPickup : MonoBehaviour, IPickup
{
    [SerializeField] private GameObject weaponPrefab;
    [SerializeField] private bool destroyAfterPickup = true;


    public bool CanPickup(GameObject picker)
    {
        if (weaponPrefab == null)
        {
            Debug.Log("weaponPrefab NULL");
            return false;
        }
        
        var equipper = picker.GetComponentInParent<IWeaponEquipper>();
        return equipper != null && equipper.CanEquip(weaponPrefab);
        
    }

    public void Pickup(GameObject picker)
    {
        Debug.Log($"Picking weapon: {weaponPrefab.name}");
        var equipper = picker.GetComponentInParent<IWeaponEquipper>();
        if (equipper == null || weaponPrefab == null) return;

        equipper.Equip(weaponPrefab);
        if (destroyAfterPickup)
        {
            Destroy(gameObject);
        }
        Debug.Log("Weapon Equipped");
    }
}