using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class WeaponPickup : MonoBehaviour, IPickup
{
    [SerializeField] private bool destroyAfterPickup = true;

    private GameObject currentPicker;

    [Header("UpdateUI")] 
    private Image weaponBG;
    private VisualEffect fireUIEffect;
    private VisualEffect iceUIEffect;
    [SerializeField] private Color newColor;
    [SerializeField]private float newColorDuration = 1f;
    

    public bool CanPickup(GameObject picker)
    {
        var equipper = picker.GetComponentInParent<IWeaponEquipper>();
        currentPicker = picker;
        return equipper != null && equipper.CanEquip(this.gameObject);
    }

    public void Pickup(GameObject picker)
    {
        Debug.Log($"Picking weapon: {this.gameObject.name}");
        var equipper = picker.GetComponentInParent<IWeaponEquipper>();
        if (equipper == null) return;

        // var modRuntimeState = picker.GetComponentInParent<ModifierRuntimeState>();

        if (ModifierRuntimeState.Instance.isModified)
        {
            AssignUIElements();

            StartCoroutine(ChangeUIColor());

            fireUIEffect.Reinit();
            iceUIEffect.Reinit();

            ModifierRuntimeState.Instance.isModified = false;
        }



        equipper.Equip(this.gameObject);
        /*
        if (destroyAfterPickup)
        {
            Destroy(gameObject);

            Debug.Log("Weapon Equipped");
        }*/
    }

    private void AssignUIElements()
    {
        weaponBG = currentPicker.GetComponentInChildren<ModifierRuntimeState>().weaponBG;
        newColorDuration = currentPicker.GetComponentInChildren<ModifierRuntimeState>().newColorDuration;
        fireUIEffect = currentPicker.GetComponentInChildren<ModifierRuntimeState>().fireUIEffect;
        iceUIEffect = currentPicker.GetComponentInChildren<ModifierRuntimeState>().iceUIEffect;
       /* 
        if (ModifierRuntimeState.Instance.isIce)
        {
            newColor = currentPicker.GetComponentInChildren<ModifierRuntimeState>().iceNewColor;
        }
        else
        {
            newColor = currentPicker.GetComponentInChildren<ModifierRuntimeState>().fireNewColor;
        }
           */ 
        
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
        /*
        if (destroyAfterPickup)
            Destroy(gameObject);*/
    }
}