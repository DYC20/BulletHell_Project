using UnityEngine;

public interface IEnemyMoveSpeed
{
    float MoveSpeed { get; set; }
}

public interface IEnemyFireInterval
{
    float FireInterval { get; set; }
}

public interface IWeaponPivot
{
    Transform WeaponPivot { get; set; }
}
