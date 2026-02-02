using UnityEngine;

public class PlayerCollisionTest : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D c)
    {
        //Debug.LogWarning($"BLOCKED BY: {c.collider.gameObject.name} | collider={c.collider.GetType().Name} | layer={LayerMask.LayerToName(c.collider.gameObject.layer)}");
    }

}
