using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class PlayerDeathHandler : MonoBehaviour
{
    //[SerializeField] private int lives = 3;
    [SerializeField] private Transform respawnPoint;
    
    private PlayerInput playerInput;
    private PlayerWeaponController pwc;
    private Rigidbody2D rb;
    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
        playerInput = GetComponent<PlayerInput>();
        pwc = GetComponent<PlayerWeaponController>();
    }

    public void OnPlayerDied()
    {
       /* lives--;

        if (lives >= 0)
        {
            transform.position = respawnPoint.position;
           // health.RestoreFull();
        }
        else*/
        {
            Debug.Log("Game Over");
            // disable input, show UI, etc.
            
            //Destroy(this.gameObject);
            
            // 1. Disable input (THIS is the key line)
            playerInput.enabled = false;
            pwc.SetCanFire(false);

            // 2. Stop movement immediately
            if (rb)
                rb.linearVelocity = Vector2.zero;

            // disable collisions
            GetComponent<Collider2D>().enabled = false;

            // 4. Optional: play animation, VFX, etc.
        }
    }
}
