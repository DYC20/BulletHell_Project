using UnityEngine;
using UnityEngine.VFX;

public class ModifierPickup : MonoBehaviour, IPickup
{
    [SerializeField] private ModifierSO modifierConfig;
    [SerializeField] private bool destroyAfterPickup = true;

    private GameObject currentPicker;

    [SerializeField] private bool isIce;

    [Header("RevolverFX")] 
    [SerializeField] private GameObject revolverFireEffect;
    [SerializeField] private GameObject revolverIceEffect;
    [SerializeField] private Sprite FireRevolverSprite;
    [SerializeField] private Sprite IceRevolverSprite;
    private bool isRevolver;
    
     
    [Header("ShotgunFX")] 
    [SerializeField] private GameObject shotgunFireEffect;
    [SerializeField] private GameObject shotgunIceEffect;
    [SerializeField] private Sprite FireShotgunSprite;
    [SerializeField] private Sprite IceShotgunSprite;
    private bool isShotgun;
     
    [Header("GrenadeFX")] 
    [SerializeField] private GameObject grenadeFireEffect;
    [SerializeField] private GameObject grenadeIceEffect;
    [SerializeField] private Sprite FireGrenadeSprite;
    [SerializeField] private Sprite IceGrenadeSprite;
    private bool isGrenade;
     
    private Transform weaponFXtf;
    private GameObject currentWeaponFX;
    private Sprite newSprite;

    public bool CanPickup(GameObject picker)
    {
        if (picker == null || modifierConfig == null) return false;
        
        currentPicker = picker;
        
        var root = picker.transform.root;

        var set = root.GetComponentInChildren<ProjectileModifierSet>(true);
        if (set == null) return false;

        var weapon = root.GetComponentInChildren<IWeaponProjectileBase>(true);
        if (weapon == null || weapon.BaseConfig == null) return false;
        
        return true;
    }

    public void Pickup(GameObject picker)
    {
        VisualEffect visuals = GetComponentInChildren<VisualEffect>();
        visuals.Stop();

        if (picker == null) Debug.Log("Picker Null");
        if (modifierConfig == null) Debug.Log("Modifier Config is null");

        if (picker == null || modifierConfig == null) return;

        ModifierRuntimeState.Instance.SetIce(isIce);
        ModifierRuntimeState.Instance.SetModifiedState(true);
        ModifierRuntimeState.Instance.AnimateUIToModifier(isIce);
        
        var root = picker.transform.root;

        var set = root.GetComponentInChildren<ProjectileModifierSet>(true);
        if (set == null) return;

        var weapon = root.GetComponentInChildren<IWeaponProjectileBase>(true);
        if (weapon == null || weapon.BaseConfig == null) return;

        weaponFXtf = weapon.WeaponFXtf;
        
        Transform bulletDrum = ((Component)weapon).transform.Find("VisualRoot/BulletDrum");
        Component weaponComponent = (Component)weapon;

        Transform mainVisual = weaponComponent.transform.Find("VisualRoot/MainVisual");

        if (mainVisual == null)
        {
            Debug.LogError("Could not find VisualRoot/MainVisual");
            return;
        }

        SpriteRenderer weaponRenderer = mainVisual.GetComponent<SpriteRenderer>();

        if (weaponRenderer == null)
        {
            Debug.LogError("MainVisual has no SpriteRenderer");
            return;
        }

        weaponRenderer.sprite = newSprite;
        /*
        if (bulletDrum != null)
        {
            SpriteRenderer drumRenderer = bulletDrum.GetComponent<SpriteRenderer>();
            drumRenderer.enabled = false;
        }
        */
        isRevolver = weapon.Revolver;
        isShotgun = weapon.Shotgun;
        isGrenade = weapon.Grenade;

        AmmoType currentAmmo = weapon.BaseConfig.ammoType;
        set.SetModifierFor(currentAmmo, modifierConfig);

        if (isRevolver)
        {
            if (isIce)
            {
                AssignWeaponFX(revolverIceEffect);
                newSprite = IceRevolverSprite;
                ModifierRuntimeState.Instance.fireUIEffect.Reinit();
                ModifierRuntimeState.Instance.iceUIEffect.Play();
            }
            else
            {
                AssignWeaponFX(revolverFireEffect);
                newSprite = FireRevolverSprite;
                ModifierRuntimeState.Instance.fireUIEffect.Play();
                ModifierRuntimeState.Instance.iceUIEffect.Reinit();
            }
        }
     
        if (isShotgun)
        {
            if (isIce)
            {
                AssignWeaponFX(shotgunIceEffect);
                newSprite = IceShotgunSprite;
                ModifierRuntimeState.Instance.fireUIEffect.Reinit();
                ModifierRuntimeState.Instance.iceUIEffect.Play();
            }
            else
            {
                AssignWeaponFX(shotgunFireEffect);
                newSprite = FireShotgunSprite;
                ModifierRuntimeState.Instance.fireUIEffect.Play();
                ModifierRuntimeState.Instance.iceUIEffect.Reinit();
            }
        }

        if (isGrenade)
        {
            if (isIce)
            {
                AssignWeaponFX(grenadeIceEffect);
                newSprite = IceGrenadeSprite;
                ModifierRuntimeState.Instance.fireUIEffect.Reinit();
                ModifierRuntimeState.Instance.iceUIEffect.Play();
            }
            else
            {
                AssignWeaponFX(grenadeFireEffect);
                newSprite = FireGrenadeSprite;
                ModifierRuntimeState.Instance.fireUIEffect.Play();
                ModifierRuntimeState.Instance.iceUIEffect.Reinit();
            }
        }

        weaponRenderer.sprite = newSprite;

        if (destroyAfterPickup)
            Destroy(gameObject);
    }

    private void AssignWeaponFX(GameObject visuals)
    {
        for (int i = weaponFXtf.childCount - 1; i >= 0; i--)
        {
            Destroy(weaponFXtf.GetChild(i).gameObject);
        }

        currentWeaponFX = Instantiate(visuals, weaponFXtf, false);
        if ( currentWeaponFX != null)
        {
            Debug.LogWarning("instenciiated Object:" + currentWeaponFX.name);
        }
    }
}