using UnityEngine;
using UnityEngine.VFX;

public class EnemieDeathHandler : MonoBehaviour
{
    [SerializeField] private GameObject deathSmokePrefab;
    private Rigidbody2D rb;
  

    public void OnEnemyDied()
    {

            Debug.Log("Enemy Died");
            // disable input, show UI, etc.
            
            

            // 2. Stop movement immediately
            if (rb)
                rb.linearVelocity = Vector2.zero;

            // disable collisions
            GetComponent<Collider2D>().enabled = false;

            // 4. Optional: play animation, VFX, etc.
            if (deathSmokePrefab)
            {
                Instantiate(deathSmokePrefab, transform.position, Quaternion.identity);
            }
        
        Destroy(this.gameObject);
    }
}
