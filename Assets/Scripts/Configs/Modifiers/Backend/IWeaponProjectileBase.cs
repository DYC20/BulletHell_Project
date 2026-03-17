using UnityEngine;

public interface IWeaponProjectileBase
{
    ProjectileConfigSO BaseConfig { get; }
    Transform WeaponFXtf { get; }
}
