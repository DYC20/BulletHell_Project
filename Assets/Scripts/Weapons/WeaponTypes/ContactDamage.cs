
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ContactDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float hitCooldown = 0.5f;

    [Header("Targeting")]
    [Tooltip("Only damage targets on these layers.")]
    [SerializeField] private LayerMask targetMask = ~0;

    [Tooltip("If true, won't damage targets on the same team.")]
    [SerializeField] private bool preventFriendlyFire = true;

    [Tooltip("If set, uses this as the attacker team. Otherwise, tries to read Team from Health on this GameObject.")]
    [SerializeField] private Teams overrideAttackerTeam = Teams.Enemy;
    [SerializeField] private bool useOverrideTeam = true;

    [Header("Runtime")]
    [SerializeField] private bool damageEnabled = true;

    private float nextHitTime;
    private Teams attackerTeam;

    public void SetDamageEnabled(bool enabled) => damageEnabled = enabled;

    private void Awake()
    {
        // Decide attacker team:
        if (useOverrideTeam)
        {
            attackerTeam = overrideAttackerTeam;
        }
        else
        {
            // If you have Health on the attacker, we can infer team from it.
            var myHealth = GetComponentInParent<Health>();
            attackerTeam = myHealth != null ? myHealth.Team : overrideAttackerTeam;
        }

        // Ensure trigger is set (contact damage needs trigger volume)
        var col = GetComponent<Collider2D>();
        if (!col.isTrigger)
            Debug.LogWarning($"{name}: ContactDamage works best with a Trigger collider (isTrigger = true).");
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!damageEnabled) return;
        if (Time.time < nextHitTime) return;

        // Layer filter
        if (((1 << other.gameObject.layer) & targetMask.value) == 0)
            return;

        // Find IDamageable on the other object or its parents (common setup)
        var damageable = other.GetComponentInParent<IDamageable>();
        if (damageable == null) return;

        // Friendly fire check
        if (preventFriendlyFire && damageable.Team == attackerTeam)
            return;

        // Apply damage (instigator is this GameObject)
        damageable.TakeDamage(damage, gameObject);

        nextHitTime = Time.time + hitCooldown;
    }
}

/*
    [SerializeField] private float damage = 10f;
    [SerializeField] private float hitCooldown = 0.5f;
    [SerializeField] private string playerTag = "Player";

    private float nextHitTime;
    private bool enabledDamage = true;

    public void SetDamageEnabled(bool enabled) => enabledDamage = enabled;
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!enabledDamage) return;
        if (Time.time < nextHitTime) return;
        if (!other.CompareTag(playerTag)) return;

        var hp = other.GetComponent<Health>();
        if (hp == null) return;

        hp.TakeDamage(damage);
        nextHitTime = Time.time + hitCooldown;
    }
}
*/
