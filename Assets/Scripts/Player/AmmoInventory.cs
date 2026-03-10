using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmmoInventory : MonoBehaviour, IAmmoReceiver, IAmmoConsumer
{
    [Serializable]
    public struct AmmoEntry
    {
        public AmmoType type;
        public int amount;
        public int maxAmount; // 0 or negative = unlimited cap
    }
    
    [Header("Starting Ammo")]
    [SerializeField] private AmmoEntry[] startingAmmo;
    
    [Header("UI")]
    [SerializeField] private Slider ammoSlider;

    private readonly Dictionary<AmmoType, AmmoEntry> _ammo = new();
    
    private AmmoType _displayedAmmoType;

    private void Awake()
    {
        _ammo.Clear();

        // Initialize all types that appear in startingAmmo
        foreach (var e in startingAmmo)
            _ammo[e.type] = e;

        // Optional: ensure all enum values exist (so HasAmmo works even if not listed)
        foreach (AmmoType t in Enum.GetValues(typeof(AmmoType)))
        {
            if (!_ammo.ContainsKey(t))
                _ammo[t] = new AmmoEntry { type = t, amount = 0, maxAmount = 0 };
            UpdateAmmoValue();
        }
    }

    public int Get(AmmoType type)
    {
        return _ammo.TryGetValue(type, out var e) ? e.amount : 0;
    }

    // IAmmoReceiver
    public bool CanReceiveAmmo(AmmoType type, int amount)
    {
        if (amount <= 0) return false;

        if (!_ammo.TryGetValue(type, out var e))
            return true;

        if (e.maxAmount <= 0) // unlimited cap
            return true;

        return e.amount < e.maxAmount;
    }

    // IAmmoReceiver
    public void ReceiveAmmo(AmmoType type, int amount)
    {
        if (!CanReceiveAmmo(type, amount)) return;

        if (!_ammo.TryGetValue(type, out var e))
            e = new AmmoEntry { type = type, amount = 0, maxAmount = 0 };

        e.amount += amount;

        if (e.maxAmount > 0)
            e.amount = Mathf.Min(e.amount, e.maxAmount);

        _ammo[type] = e;
        
        if (type == _displayedAmmoType)
            UpdateAmmoValue();
        
    }

    // IAmmoConsumer
    public bool HasAmmo(AmmoType type, int amount)
    {
        if (amount <= 0) return true;
        return Get(type) >= amount;
    }

    // IAmmoConsumer
    public bool TryConsumeAmmo(AmmoType type, int amount)
    {
        if (amount <= 0) return true;
        if (!HasAmmo(type, amount)) return false;

        var e = _ammo[type];
        e.amount -= amount;
        if (e.amount < 0) e.amount = 0;
        _ammo[type] = e;
        
        if (type == _displayedAmmoType)
            UpdateAmmoValue();

        return true;
    }
    
    public void SetDisplayedAmmoType(AmmoType type)
    {
        _displayedAmmoType = type;
        UpdateAmmoValue();
    }

    private void UpdateAmmoValue()
    {
        if (ammoSlider == null) return;
        ammoSlider.value = Get(_displayedAmmoType);
        //ammoText.text = Get(_displayedAmmoType).ToString();
        //ammoTextShadow.text = Get(_displayedAmmoType).ToString();
    }
}
