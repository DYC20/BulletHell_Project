using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Health : MonoBehaviour, IDamageable, IHealable
{
    [Header("Setup")] [SerializeField] private Teams team;
    [SerializeField] private float maxHealth = 5f;
    [SerializeField] private Slider healthSlider;


    [Header("FullScreen Material")] [SerializeField]
    private Material FullScreenPlayerHit;

    private string controlEffect = "_ControlEffect";

    public Teams Team => team;
    public float MaxHealth => maxHealth;
    public float CurrentHealth => _hp;
    public bool IsDead => _hp <= 0f;

    [Header("Events")] public UnityEvent onDeath;
    public UnityEvent<float, GameObject> onDamaged; // damage amount, instigator

    private float _hp;

    private void Awake()
    {
        ResetHealth();
        if (healthSlider == null)
        {
            healthSlider = GetComponent<Slider>();

        }

        healthSlider.value = CurrentHealth;
    }

    /// <summary>
    /// Apply damage to this entity.
    /// </summary>
    public void TakeDamage(float amount, GameObject instigator)
    {
        if (IsDead) return;
        if (amount <= 0f) return;

        _hp -= amount;
//Debug.Log("Player Took Damage");
        onDamaged?.Invoke(amount, instigator);
        if (healthSlider != null)
            healthSlider.value = CurrentHealth;

        if (_hp <= 3f)
        {
            Debug.Log("HP < 3");
            StartCoroutine(ActivateFullScreenHealth());
        }

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

        if (_hp > 3f)
            StartCoroutine(DeactivateFullScreenHealth());
        Debug.Log("Player Healed");
    }

    IEnumerator ActivateFullScreenHealth()
    {
        float duration = 1f;
        float timer = 0f;
        //Color startColor = weaponBG.color;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            FullScreenPlayerHit.SetFloat(controlEffect, t);

            yield return null;
        }
    }
    
     IEnumerator DeactivateFullScreenHealth()
        {
            float duration = 1f;
            float timer = 0f;
            //Color startColor = weaponBG.color;
    
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;
                t = Mathf.SmoothStep(1f, 0f, t);
    
                FullScreenPlayerHit.SetFloat(controlEffect, t);
    
                yield return null;
            }
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
