using UnityEngine;

public class PlayerGroundState : MonoBehaviour
{
    public bool IsOnHighland { get; private set; }

    public void SetHighland(bool value) => IsOnHighland = value;

}
