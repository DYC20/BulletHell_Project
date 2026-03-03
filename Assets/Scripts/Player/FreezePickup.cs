using UnityEngine;

public class FreezePickup : MonoBehaviour, IPickup
{
    [SerializeField] private ModifierSO modifierConfig;
    [SerializeField] private bool destroyAfterPickup = true;

    public bool CanPickup(GameObject picker)
    {
        if (modifierConfig == null) return false;

        var set = picker.GetComponentInParent<ProjectileModifierSet>();
        if (set == null) return false;

        // Optional: prevent picking same upgrade twice
        return !set.HasModifier(modifierConfig);
    }

    public void Pickup(GameObject picker)
    {
        var set = picker.GetComponentInParent<ProjectileModifierSet>();
        if (set == null || modifierConfig == null) return;

        set.AddModifier(modifierConfig);

        if (destroyAfterPickup)
            Destroy(gameObject);
    }
}
