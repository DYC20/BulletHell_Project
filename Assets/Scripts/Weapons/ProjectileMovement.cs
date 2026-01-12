using UnityEngine;

public class ProjectileMovement : MonoBehaviour
{
 

    public void Apply(Rigidbody2D rb, ProjectileConfigSO config, Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * config.speed;
    }
}