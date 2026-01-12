using UnityEngine;

public interface IPickup
{
    bool CanPickup(GameObject picker);
    void Pickup(GameObject picker);
}
