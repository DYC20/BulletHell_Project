using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerWeaponController : MonoBehaviour, IWeaponEquipper
{
    [Header("Weapon")]

    [SerializeField] private bool canFire = true;
    [SerializeField] private Transform weaponSocket;
    [SerializeField] private float gunDistance = 0.34f;
    [SerializeField] public WeaponBase equippedWeapon;
    [SerializeField] private AmmoInventory ammoInventory;
    [SerializeField] private Image weaponImage;
    [SerializeField] public ShockWave shockWave;

    private ModifierRuntimeState modifierRunTimeState;
    private ScriptableObject currentModifier;

    private bool _isFiring;
    private bool notFirstWeapon = false;
    public Transform CurrentFirePoint { get; private set; }

    private void Awake()
    {
        if (modifierRunTimeState == null)
            modifierRunTimeState = GetComponent<ModifierRuntimeState>();
        if (ammoInventory == null)
            ammoInventory = GetComponentInParent<AmmoInventory>();
        if (equippedWeapon == null)
            equippedWeapon = GetComponentInChildren<WeaponBase>(includeInactive: true);
        if (equippedWeapon != null && weaponImage != null)
        {
            SpriteRenderer weaponRenderer = equippedWeapon.GetComponentInChildren<SpriteRenderer>(true);

            if (weaponRenderer != null)
            {
                weaponImage.sprite = equippedWeapon.GetComponent<SimplePistol_Waepon>().UIImage;
            }
            else
            {
                Debug.LogWarning("PlayerWeaponController: No SpriteRenderer found under equipped weapon: " + equippedWeapon.name);
            }
        }

        
        if (equippedWeapon == null)
            canFire = false;
        if (equippedWeapon != null)
            Equip(equippedWeapon);
        currentModifier = modifierRunTimeState.Modifier;
        modifierRunTimeState.ClearModifier(currentModifier);
        // else
        //Debug.LogWarning("PlayerWeaponController: No WeaponBase found under player.");
    }

    public void Equip(WeaponBase weapon)
    {
        if (equippedWeapon != null)
        {
            GameObject oldWeapon = equippedWeapon.gameObject;
            modifierRunTimeState.ClearModifier(currentModifier);
            if (notFirstWeapon)
            {
              Destroy( oldWeapon);  
            }

            notFirstWeapon = true;
            /* Debug.Log($" is Null Equipped weapon: {equippedWeapon.name}");
            equippedWeapon.transform.SetParent(null, true);
            equippedWeapon.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            equippedWeapon.GetComponent<Collider2D>().enabled = true;*/
        }

        canFire = true;
        equippedWeapon = weapon;
        Debug.Log($" no Null Equipped weapon: {equippedWeapon.name}");
        
        equippedWeapon.ResetToDefaultProjectileData();
        
        // clear the modifier that keeps overriding the pool/config
        var modifierSet = GetComponentInParent<ProjectileModifierSet>();
        if (modifierSet != null)
        {
            modifierSet.ClearAllModifiers();
        }
        if (modifierRunTimeState != null)
        {
            modifierRunTimeState.SetModifiedState(false);
            modifierRunTimeState.AnimateUIToDefault();

            if (modifierRunTimeState.fireUIEffect != null)
                modifierRunTimeState.fireUIEffect.Reinit();

            if (modifierRunTimeState.iceUIEffect != null)
                modifierRunTimeState.iceUIEffect.Reinit();
        }
        
        equippedWeapon.SetOwner(gameObject, Teams.Player);
        equippedWeapon.GetComponent<Collider2D>().enabled = false;
        equippedWeapon.gameObject.transform.parent = weaponSocket;
        equippedWeapon.gameObject.transform.localPosition = Vector3.up * gunDistance;
        equippedWeapon.gameObject.transform.localRotation = Quaternion.identity;
        

        if (ammoInventory != null)
            ammoInventory.SetDisplayedAmmoType(equippedWeapon.GetCurrentAmmoType());
    }
    
    public void RefreshAmmoUI()
    {
        if (equippedWeapon == null || ammoInventory == null) return;
        ammoInventory.SetDisplayedAmmoType(equippedWeapon.GetCurrentAmmoType());
    }

    // IWeaponEquipper
    public bool CanEquip(GameObject weaponPrefab)
    {
        return weaponPrefab != null && weaponPrefab.GetComponent<WeaponBase>() != null;
    }

    // IWeaponEquipper
    public void Equip(GameObject weaponPrefab)
    {
        Debug.Log($"Equipping prefab: {weaponPrefab.name}");
        if (!CanEquip(weaponPrefab)) return;

        if (weaponSocket == null)
            weaponSocket = transform;

        // Destroy old equipped weapon instance (simple for now)
        /*if (equippedWeapon != null)
            Destroy(equippedWeapon.gameObject);*/

        /*var weaponGO = Instantiate(weaponPrefab, weaponSocket);
        weaponGO.transform.localPosition = Vector3.zero;
        weaponGO.transform.localRotation = Quaternion.identity;*/

        var weapon = weaponPrefab.GetComponent<WeaponBase>();
        Equip(weapon);

        weaponImage.sprite = equippedWeapon.GetComponent<SimplePistol_Waepon>().UIImage;
        Debug.Log("UI weapon sprite changed");
        CurrentFirePoint = (weapon != null) ? weapon.FirePoint : null;
    }


    // Input System: Action name "Fire" => method "OnFire"
    public void OnFire(InputValue value)
    {
            _isFiring = value.isPressed;
        //Debug.LogWarning("isFiring");
    }

    private void LateUpdate()
    {
        if (_isFiring)
            equippedWeapon?.TryFire();
    }
    
    ////////////
    /// canFure---operators
    ///////////
    public bool CanFireGetSet
    {
        get { return canFire; }
        set { canFire = value; }
    }

    // Useful for UnityEvents / Timeline Signals
    public void SetCanFire(bool value)
    {
        canFire = value;
//        Debug.LogWarning("Player canFire set to: " + canFire);
    }

    public void DisableFire()
    {
        SetCanFire(false);
    }

    public void EnableFire()
    {
        SetCanFire(true);
    }
}