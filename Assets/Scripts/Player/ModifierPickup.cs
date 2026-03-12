using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines.Interpolators;
using UnityEngine.UI;
using UnityEngine.VFX;

public class ModifierPickup : MonoBehaviour, IPickup
{
    [SerializeField] private ModifierSO modifierConfig;
    [SerializeField] private bool destroyAfterPickup = true;

    private GameObject currentPicker;

    [Header("UpdateUI")] 
     private Image weaponBG;
     private VisualEffect fireUIEffect;
     private VisualEffect iceUIEffect;
     private Color newColor;
     private float newColorDuration;

     [SerializeField] private bool isIce;

    public bool CanPickup(GameObject picker)
    {
        if (picker == null || modifierConfig == null) return false;
        
        currentPicker = picker;
        
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
        VisualEffect visuals = GetComponentInChildren<VisualEffect>();
             visuals.Stop();
        if (picker == null || modifierConfig == null) return;

        AssignUIElements();
        
        ModifierRuntimeState.Instance.SetIce(isIce);
        ModifierRuntimeState.Instance.SetModifiedState(true);
        
        var root = picker.transform.root;

        var set = root.GetComponentInChildren<ProjectileModifierSet>(true);
        if (set == null) return;

        var weapon = root.GetComponentInChildren<IWeaponProjectileBase>(true);
        if (weapon == null || weapon.BaseConfig == null) return;

        AmmoType currentAmmo = weapon.BaseConfig.ammoType;
        set.SetModifierFor(currentAmmo, modifierConfig);
        //Debug.Log("Picker:" + picker.name);
        StartCoroutine(ChangeUIColor());

        if (isIce)
        {
            fireUIEffect.Reinit();
            iceUIEffect.Play();
        }
        else
        {
            fireUIEffect.Play();
            iceUIEffect.Reinit();
        }

        
        
        //Debug.Log($"Picked {modifierConfig.name} for ammo={currentAmmo}. Current mod = {set.GetModifierFor(currentAmmo)?.name}");

        
    }

    private void AssignUIElements()
    {
        weaponBG = currentPicker.GetComponentInChildren<ModifierRuntimeState>().weaponBG;
        newColorDuration = currentPicker.GetComponentInChildren<ModifierRuntimeState>().newColorDuration;
        fireUIEffect = currentPicker.GetComponentInChildren<ModifierRuntimeState>().fireUIEffect;
        iceUIEffect = currentPicker.GetComponentInChildren<ModifierRuntimeState>().iceUIEffect;
        
        if (isIce)
        {
            newColor = currentPicker.GetComponentInChildren<ModifierRuntimeState>().iceNewColor;
        }
        else
        {
            newColor = currentPicker.GetComponentInChildren<ModifierRuntimeState>().fireNewColor;
        }
            
        
    }

    IEnumerator ChangeUIColor()
    {
        float timer = 0f;
        Color startColor = weaponBG.color;

        while (timer < newColorDuration)
        {
            timer += Time.deltaTime;
            float t = timer / newColorDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            weaponBG.color = Color.Lerp(startColor, newColor, t);

            yield return null;
        }
        
        weaponBG.color = newColor;
        
        if (destroyAfterPickup)
            Destroy(gameObject);
    }
 
}