using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour, IDamageable, IHealable
{
    [Header("Setup")]
    [SerializeField] private Teams team;
    [SerializeField] private float maxHealth = 5f;

    public Teams Team => team;
    public float MaxHealth => maxHealth;
    public float CurrentHealth => _hp;
    public bool IsDead => _hp <= 0f;

    [Header("Events")]
    public UnityEvent onDeath;
    public UnityEvent<float, GameObject> onDamaged; // damage amount, instigator

    private float _hp;

    private void Awake()
    {
        ResetHealth();
    }

    /// <summary>
    /// Apply damage to this entity.
    /// </summary>
    public void TakeDamage(float amount, GameObject instigator)
    {
        if (IsDead) return;
        if (amount <= 0f) return;

        _hp -= amount;
Debug.Log("Player Took Damage");
        onDamaged?.Invoke(amount, instigator);

        if (_hp <= 0f)
        {
            _hp = 0f;
            onDeath?.Invoke();
        }
    }

    /// <summary>
    /// Fully restore health (useful for respawn / pooling).
    /// </summary>
    public void ResetHealth()
    {
        _hp = maxHealth;
    }

    /// <summary>
    /// Optional: heal without exceeding max.
    /// </summary>
    
    public bool CanHeal(float amount)
    {
        if (IsDead) return false;
        if (amount <= 0f) return false;
        return _hp < maxHealth;
    }
    public void Heal(float amount)
    {
        if (IsDead) return;
        if (!CanHeal(amount)) return;

        _hp = Mathf.Min(_hp + amount, maxHealth);
        Debug.Log("Player Healed");
    }
 }   
    
    
    
    /*
    [SerializeField] private Teams team;
    [SerializeField] private float maxHealth = 5f;

    public Teams Team => team;

    public UnityEvent onDeath;

    private float _hp;

    private void Awake()
    {
        _hp = maxHealth;
    }

    public void TakeDamage(float amount, GameObject instigator)
    {
        if (_hp <= 0f) return;

        _hp -= amount;
        if (_hp <= 0f)
        {
            _hp = 0f;
            onDeath?.Invoke();
            Debug.Log(Team + "Took Damage");
        }
    }
    */
