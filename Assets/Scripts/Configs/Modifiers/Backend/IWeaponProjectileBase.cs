using UnityEngine;

public interface IWeaponProjectileBase
{
    ProjectileConfigSO BaseConfig { get; }
    Transform WeaponFXtf { get; }
    
    Transform FirePoint { get; }
    
    bool Revolver { get; }
    
    bool Shotgun { get; }
    
    bool Grenade { get; }

    Sprite WeaponImage { get;}
}
