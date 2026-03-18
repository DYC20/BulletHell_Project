using UnityEngine;

public interface IWeaponProjectileBase
{
    ProjectileConfigSO BaseConfig { get; }
    Transform WeaponFXtf { get; }
    
    bool Revolver { get; }
    
    bool Shotgun { get; }

    Sprite WeaponImage { get;}
}
