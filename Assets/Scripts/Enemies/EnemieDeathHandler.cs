using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class EnemieDeathHandler : MonoBehaviour
{
    [SerializeField] private List<GameObject> deathSmokePrefab;
    [SerializeField] private GameObject ammoPrefab;
    [SerializeField] private GameObject healthPrefab;
    [SerializeField] private int dropProbabillity = 9;
    [SerializeField] private int healthDropProbability = 3;
    private Rigidbody2D rb;
    private bool droppedAmmo = false;
    private bool droppedHealth = false;
  

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
            for (int FX = 0; FX < deathSmokePrefab.Count; FX++)
            {
                GameObject deathFX = deathSmokePrefab[FX];
                Instantiate(deathFX, transform.position, Quaternion.identity);
            }
            /*
            if (deathSmokePrefab)
            {
                Instantiate(deathSmokePrefab, transform.position, Quaternion.identity);
            }
            */
            AmmoDroppProbability();
            if (droppedAmmo)
            {
                Instantiate(ammoPrefab, transform.position, Quaternion.identity);
            }
            if (droppedHealth)
            {
                Instantiate(healthPrefab, transform.position, Quaternion.identity);
            }
        
        Destroy(this.gameObject);
    }

    private void AmmoDroppProbability()
    {
        int rand = UnityEngine.Random.Range(1, dropProbabillity + 1);
        
        if (rand == dropProbabillity)
        {
            int ammoOrHealthPro = UnityEngine.Random.Range(1, healthDropProbability +1 );
            if (ammoOrHealthPro == healthDropProbability)
            {
                droppedHealth = true;
            }
            else
            {
                droppedAmmo = true;
            }
        }
        Debug.LogWarning("AmmoDroppProbability =" + rand);
    }
}
