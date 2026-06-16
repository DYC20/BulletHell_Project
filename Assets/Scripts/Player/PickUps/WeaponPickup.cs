using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class WeaponPickup : MonoBehaviour, IPickup
{
    [SerializeField] private bool destroyAfterPickup = true;
    [SerializeField] private GameObject interactableGO;
    [SerializeField] private SpriteRenderer weaponRenderer;

    private GameObject currentPicker;
    private IInteractable interactable;
    private PlayerRendererGetter playerRendererGetter;

    [Header("UpdateUI")] 
    private Image weaponBG;
    private VisualEffect fireUIEffect;
    private VisualEffect iceUIEffect;
    [SerializeField] private SpriteRenderer shadow;
    [SerializeField] private Color newColor;
    [SerializeField]private float newColorDuration = 1f;

    private void Start()
    {
        interactable = interactableGO.GetComponent<IInteractable>();
    }

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
        
        //make sorting layer as player's on weapon pickup
        playerRendererGetter = picker.GetComponentInParent<PlayerRendererGetter>();
        Debug.Log("Player Renderer Getter:" + playerRendererGetter.name);
        
        var playerSortingLayer = playerRendererGetter.GetRenderLayer;
        int playerSortingOrder = playerRendererGetter.GetSortingOrder;
        
        weaponRenderer.sortingLayerName = playerSortingLayer;
        weaponRenderer.sortingOrder = playerSortingOrder + 1;
        Debug.Log("Player Sorting Layer:" + playerSortingLayer);
        
        
        if (equipper == null) return;

        if (interactableGO != null && interactable != null)
        {
            Destroy(interactableGO);
        }
            
        // var modRuntimeState = picker.GetComponentInParent<ModifierRuntimeState>();

        if (ModifierRuntimeState.Instance.isModified)
        {
            AssignUIElements();

            StartCoroutine(ChangeUIColor());

            fireUIEffect.Reinit();
            iceUIEffect.Reinit();

            ModifierRuntimeState.Instance.isModified = false;
        }


        shadow.gameObject.SetActive(false);
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