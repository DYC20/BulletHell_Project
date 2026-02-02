using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeaponController : MonoBehaviour,IWeaponEquipper
{
    [Header("Weapon")]
    [SerializeField] private Transform weaponSocket;
    [SerializeField] private WeaponBase equippedWeapon;

    private bool _isFiring;
    public Transform CurrentFirePoint { get; private set; }
    
    private void Awake()
    {
        if (equippedWeapon == null)
            equippedWeapon = GetComponentInChildren<WeaponBase>(includeInactive: true);

        if (equippedWeapon != null)
            Equip(equippedWeapon);
       // else
            //Debug.LogWarning("PlayerWeaponController: No WeaponBase found under player.");
    }
    
    public void Equip(WeaponBase weapon)
    {
        equippedWeapon = weapon;
        if (equippedWeapon != null)
        {
            //Debug.Log($"Equipped weapon: {equippedWeapon.name}");
            equippedWeapon.SetOwner(gameObject, Teams.Player);
            
        }
            
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
        if (equippedWeapon != null)
            Destroy(equippedWeapon.gameObject);

        var weaponGO = Instantiate(weaponPrefab, weaponSocket);
        weaponGO.transform.localPosition = Vector3.zero;
        weaponGO.transform.localRotation = Quaternion.identity;

        var weapon = weaponGO.GetComponent<WeaponBase>();
        Equip(weapon);
        
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
}