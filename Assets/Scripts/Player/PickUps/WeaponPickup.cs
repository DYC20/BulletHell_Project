using UnityEngine;

public class WeaponPickup : MonoBehaviour, IPickup
{
    [SerializeField] private bool destroyAfterPickup = true;


    public bool CanPickup(GameObject picker)
    {
        var equipper = picker.GetComponentInParent<IWeaponEquipper>();
        return equipper != null && equipper.CanEquip(this.gameObject);
        
    }

    public void Pickup(GameObject picker)
    {
        Debug.Log($"Picking weapon: {this.gameObject.name}");
        var equipper = picker.GetComponentInParent<IWeaponEquipper>();
        if (equipper == null) return;

        equipper.Equip(this.gameObject);
        /*if (destroyAfterPickup)
        {
            Destroy(gameObject);
        }*/
        Debug.Log("Weapon Equipped");
    }
}