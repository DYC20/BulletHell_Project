using UnityEngine;

public class AmmoPickup : MonoBehaviour, IPickup
{
    [SerializeField] private AmmoType ammoType;
    [SerializeField] private int amount = 30;

    public bool CanPickup(GameObject picker)
    {
        var ammo = picker.GetComponentInParent<IAmmoReceiver>();
        return ammo != null && ammo.CanReceiveAmmo(ammoType, amount);
    }

    public void Pickup(GameObject picker)
    {
        var ammo = picker.GetComponentInParent<IAmmoReceiver>();
       // Debug.Log(ammo == null
           // ? "Pickup: NO IAmmoReceiver found"
           // : $"Pickup: Receiver is {((MonoBehaviour)ammo).name} on {((MonoBehaviour)ammo).gameObject.name}");
        
        if (ammo == null) return;

        ammo.ReceiveAmmo(ammoType, amount);
        
        var inv = picker.GetComponentInParent<AmmoInventory>();
        if (inv != null)
           // Debug.Log($"Pickup: After ReceiveAmmo, {ammoType} = {inv.Get(ammoType)}");

        
        Destroy(gameObject); // later: pool pickups too
    }
}