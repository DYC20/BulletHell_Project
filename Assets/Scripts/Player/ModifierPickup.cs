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

    /// <summary>
    /// assigned in runtimestate
    /// </summary>
    [Header("UpdateUI")] 
     private Image weaponBG;
     private ParticleSystem weaponBGFX;
     private Gradient newFireBGFXColor;
     private Gradient newIceBGFXColor;
     private VisualEffect fireUIEffect;
     private VisualEffect iceUIEffect;
     private Color newColor;
     private float newColorDuration;

     [SerializeField] private bool isIce;

     [Header("RevolverFX")] 
     [SerializeField] private VisualEffect revolverFireEffect;
     [SerializeField] private VisualEffect revolverIceEffect;
     [SerializeField] private Sprite FireRevolverSprite;
     [SerializeField] private Sprite IceRevolverSprite;
     private bool isRevolver;
     
     [Header("ShotgunFX")] 
     [SerializeField] private VisualEffect shotgunFireEffect;
     [SerializeField] private VisualEffect shotgunIceEffect;
     [SerializeField] private Sprite FireShotgunSprite;
     [SerializeField] private Sprite IceShotgunSprite;
     private bool isShotgun;
     
     [Header("ShotgunFX")] 
     [SerializeField] private VisualEffect grenadeFireEffect;
     [SerializeField] private VisualEffect grenadeIceEffect;
     [SerializeField] private Sprite FireGrenadeSprite;
     [SerializeField] private Sprite IceGrenadeSprite;
     private bool isGrenade;
     
     private Transform weaponFXtf;
     private bool revolverOrShotgun;
     private VisualEffect currentWeaponFX;
     private Sprite newSprite;
     private ParticleSystem.ColorOverLifetimeModule colorOverLifetime;
     private ParticleSystem.ColorOverLifetimeModule defaultColorOverLifetime;

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
             if (picker == null) Debug.Log("Picker Null");
             if (modifierConfig == null) Debug.Log("Modifier Config is null");
        if (picker == null || modifierConfig == null) return;

        AssignUIElements();
        
        ModifierRuntimeState.Instance.SetIce(isIce);
        ModifierRuntimeState.Instance.SetModifiedState(true);
        
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
        
        if (bulletDrum != null)
        {
             SpriteRenderer drumRenderer = bulletDrum.GetComponent<SpriteRenderer>();
                    
                    drumRenderer.enabled = false;
        }

        isRevolver = weapon.Revolver;
        isShotgun = weapon.Shotgun;
        isGrenade = weapon.Grenade;

      
        
        AmmoType currentAmmo = weapon.BaseConfig.ammoType;
        set.SetModifierFor(currentAmmo, modifierConfig);
        //Debug.Log("Picker:" + picker.name);
        StartCoroutine(ChangeUIColor());
        if (isRevolver)
        {
               if (isIce)
                    {
                        AssignWeaponFX(revolverIceEffect);
                        newSprite = IceRevolverSprite;
                        fireUIEffect.Reinit();
                        iceUIEffect.Play();
                    }
                    else
                    {
                        AssignWeaponFX(revolverFireEffect);
                        newSprite = FireRevolverSprite;
                        fireUIEffect.Play();
                        iceUIEffect.Reinit();
                    }
        }
     
        if (isShotgun)
        {
            if (isIce)
            {
                AssignWeaponFX(shotgunIceEffect);
                newSprite = IceShotgunSprite;
                fireUIEffect.Reinit();
                iceUIEffect.Play();
            }
            else
            {
                AssignWeaponFX(shotgunFireEffect);
                newSprite = FireShotgunSprite;
                fireUIEffect.Play();
                iceUIEffect.Reinit();
            }
        }
        if (isGrenade)
        {
            if (isIce)
            {
                AssignWeaponFX(grenadeIceEffect);
                newSprite = IceGrenadeSprite;
                fireUIEffect.Reinit();
                iceUIEffect.Play();
            }
            else
            {
                AssignWeaponFX(grenadeFireEffect);
                newSprite = FireGrenadeSprite;
                fireUIEffect.Play();
                iceUIEffect.Reinit();
            }
        }
        weaponRenderer.sprite = newSprite;
        
        //Debug.Log($"Picked {modifierConfig.name} for ammo={currentAmmo}. Current mod = {set.GetModifierFor(currentAmmo)?.name}");

        
    }

    private void AssignWeaponFX(VisualEffect visuals)
    {
        for (int i = weaponFXtf.childCount - 1; i >= 0; i--)
        {
            Destroy(weaponFXtf.GetChild(i).gameObject);
        }

        currentWeaponFX = Instantiate(visuals, weaponFXtf, false);
    }

    private void AssignUIElements()
    {
        weaponBG = currentPicker.GetComponentInChildren<ModifierRuntimeState>().weaponBG;
        weaponBGFX = currentPicker.GetComponentInChildren<ModifierRuntimeState>().weaponBGFX;
        newColorDuration = currentPicker.GetComponentInChildren<ModifierRuntimeState>().newColorDuration;
        fireUIEffect = currentPicker.GetComponentInChildren<ModifierRuntimeState>().fireUIEffect;
        iceUIEffect = currentPicker.GetComponentInChildren<ModifierRuntimeState>().iceUIEffect;
        
        if (isIce)
        {
            newColor = currentPicker.GetComponentInChildren<ModifierRuntimeState>().iceNewColor;
            newIceBGFXColor = currentPicker.GetComponentInChildren<ModifierRuntimeState>().newIceBGFXColor;
        }
        else
        {
            newColor = currentPicker.GetComponentInChildren<ModifierRuntimeState>().fireNewColor;
            newFireBGFXColor = currentPicker.GetComponentInChildren<ModifierRuntimeState>().newFireBGFXColor;
        }
    }

    IEnumerator ChangeUIColor()
    {
        float timer = 0f;
        Color startColor = weaponBG.color;
        
        colorOverLifetime = weaponBGFX.colorOverLifetime;
        defaultColorOverLifetime = colorOverLifetime;
        colorOverLifetime.enabled = true;
        
        Gradient startGradient = colorOverLifetime.color.gradient;
        Gradient targetGradient = isIce ? newIceBGFXColor : newFireBGFXColor;

        while (timer < newColorDuration)
        {
            timer += Time.deltaTime;
            float t = timer / newColorDuration;
            t = Mathf.SmoothStep(0f, 1f, t);
            
            weaponBG.color = Color.Lerp(startColor, newColor, t);
            
           
            Gradient lerpedGradient = LerpGradient(startGradient, targetGradient, t);
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(lerpedGradient);

            yield return null;
        }
        
        weaponBG.color = newColor;
        
        if (destroyAfterPickup)
            Destroy(gameObject);
    }
    public Gradient LerpGradient(Gradient from, Gradient to, float t)
    {
        Gradient result = new Gradient();

        GradientColorKey[] fromColors = from.colorKeys;
        GradientColorKey[] toColors = to.colorKeys;

        GradientAlphaKey[] fromAlphas = from.alphaKeys;
        GradientAlphaKey[] toAlphas = to.alphaKeys;

        int colorCount = Mathf.Min(fromColors.Length, toColors.Length);
        int alphaCount = Mathf.Min(fromAlphas.Length, toAlphas.Length);

        GradientColorKey[] resultColors = new GradientColorKey[colorCount];
        GradientAlphaKey[] resultAlphas = new GradientAlphaKey[alphaCount];

        for (int i = 0; i < colorCount; i++)
        {
            resultColors[i] = new GradientColorKey(
                Color.Lerp(fromColors[i].color, toColors[i].color, t),
                Mathf.Lerp(fromColors[i].time, toColors[i].time, t)
            );
        }

        for (int i = 0; i < alphaCount; i++)
        {
            resultAlphas[i] = new GradientAlphaKey(
                Mathf.Lerp(fromAlphas[i].alpha, toAlphas[i].alpha, t),
                Mathf.Lerp(fromAlphas[i].time, toAlphas[i].time, t)
            );
        }

        result.SetKeys(resultColors, resultAlphas);
        return result;
    }

    public ParticleSystem.ColorOverLifetimeModule DefaultColorOverLifetime => defaultColorOverLifetime;

}