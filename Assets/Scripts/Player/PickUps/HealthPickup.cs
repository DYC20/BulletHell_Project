using UnityEngine;

public class HealthPickup : MonoBehaviour, IPickup
{
    [SerializeField] private float healAmount = 2f;

    public bool CanPickup(GameObject picker)
    {
        var heal = picker.GetComponentInParent<IHealable>();
        return heal != null && heal.CanHeal(healAmount);
    }

    public void Pickup(GameObject picker)
    {
        var heal = picker.GetComponentInParent<IHealable>();
        if (heal == null) return;

        heal.Heal(healAmount);
        Destroy(gameObject);
    }
}