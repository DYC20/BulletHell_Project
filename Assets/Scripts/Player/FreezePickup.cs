using UnityEngine;

public class FreezePickup : MonoBehaviour, IPickup
{
    [SerializeField] private FreezeModifierSO freezeConfig;
    [SerializeField] private bool destroyAfterPickup = true;

    public bool CanPickup(GameObject picker)
    {
        if (freezeConfig == null) return false;

        var set = picker.GetComponentInParent<ProjectileModifierSet>();
        if (set == null) return false;

        // Optional: prevent picking same upgrade twice
        return !set.HasModifier(freezeConfig);
    }

    public void Pickup(GameObject picker)
    {
        var set = picker.GetComponentInParent<ProjectileModifierSet>();
        if (set == null || freezeConfig == null) return;

        set.AddModifier(freezeConfig);

        if (destroyAfterPickup)
            Destroy(gameObject);
    }
}
