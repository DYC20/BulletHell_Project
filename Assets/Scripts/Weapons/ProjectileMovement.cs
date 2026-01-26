using UnityEngine;

public class ProjectileMovement : MonoBehaviour
{
 

    public void Apply(Rigidbody2D rb, ProjectileConfigSO config, Vector2 direction, float speed)
    {
        rb.linearVelocity = direction.normalized * config.speed * speed;
    }
}