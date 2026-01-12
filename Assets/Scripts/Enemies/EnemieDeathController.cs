using UnityEngine;

public class EnemieDeathController : MonoBehaviour
{
    [SerializeField] private float destroyDelay = 0.5f;
    [SerializeField] private GameObject deathVfx;

    public void OnEnemyDied()
    {
        // stop AI
        //var ai = GetComponent<EnemyAI>();
        //if (ai) ai.enabled = false;

        // disable collisions
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        // spawn VFX
        if (deathVfx)
            Instantiate(deathVfx, transform.position, Quaternion.identity);

        Destroy(gameObject, destroyDelay);
    }
}
