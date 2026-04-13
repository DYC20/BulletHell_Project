using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Health : MonoBehaviour, IDamageable, IHealable
{
    [Header("Setup")]
    [SerializeField] private Teams team;
    [SerializeField] private float maxHealth = 5f;
    [SerializeField] private RectTransform healthBarRect;
    [SerializeField] private RectTransform edgeSpriteRect;
    [SerializeField] private float leftPadding = 0f;
    [SerializeField] private float rightPadding = 0f;

    [Header("Health Bar Material")]
    [SerializeField] private Image healthBarImage;
    [SerializeField] private string healthFillProperty = "_FillAmount";

    [Header("FullScreen Material")]
    [SerializeField] private Material FullScreenPlayerHit;

    private string controlEffect = "_ControlEffect";

    public Teams Team => team;
    public float MaxHealth => maxHealth;
    public float CurrentHealth => _hp;
    public bool IsDead => _hp <= 0f;

    [Header("Events")]
    public UnityEvent onDeath;
    public UnityEvent<float, GameObject> onDamaged; // damage amount, instigator

    private float _hp;
    private Material _healthBarRuntimeMaterial;

    private void Awake()
    {
        _healthBarRuntimeMaterial = healthBarImage.material;
        ResetHealth();
        SetupHealthBarMaterial();
        UpdateHealthBarVisual();
    }

    /// <summary>
    /// Apply damage to this entity.
    /// </summary>
    public void TakeDamage(float amount, GameObject instigator)
    {
        if (IsDead) return;
        if (amount <= 0f) return;

        _hp -= amount;

        onDamaged?.Invoke(amount, instigator);
        UpdateHealthBarVisual();

        if (_hp <= 3f)
        {
            Debug.Log("HP < 3");
            StartCoroutine(ActivateFullScreenLowHealth());
        }

        if (_hp <= 0f)
        {
            _hp = 0f;
            UpdateHealthBarVisual();
            onDeath?.Invoke();
        }
    }

    /// <summary>
    /// Fully restore health (useful for respawn / pooling).
    /// </summary>
    public void ResetHealth()
    {
        _hp = maxHealth;
        UpdateHealthBarVisual();
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
        UpdateHealthBarVisual();

        if (_hp > 3f)
            StartCoroutine(DeactivateFullScreenLowHealth());

        Debug.Log("Player Healed");
    }

    private void SetupHealthBarMaterial()
    {
        if (healthBarImage == null)
            healthBarImage = GetComponent<Image>();

        if (healthBarImage == null || healthBarImage.material == null)
            return;

        _healthBarRuntimeMaterial = new Material(healthBarImage.material);
        healthBarImage.material = _healthBarRuntimeMaterial;
    }

    private void UpdateHealthBarVisual()
    {
        if (_healthBarRuntimeMaterial == null)
        {
            
            Debug.Log("_healthBarRuntimeMaterial is null");
            return;
        }
            
        ///Health bar normalized to 0-0.8, used in BarEdgeFollower
        float normalizedHealth = Mathf.Clamp01(_hp / maxHealth);
        _healthBarRuntimeMaterial.SetFloat(healthFillProperty, normalizedHealth * 0.8f);
        BarEdgeFollower edgeFollower = edgeSpriteRect.GetComponent<BarEdgeFollower>();
        edgeFollower.SetNormalized(normalizedHealth);
        
       /* if (healthBarRect != null && edgeSpriteRect != null)
        {
            float usableWidth = healthBarRect.rect.width - leftPadding - rightPadding;
            float x = leftPadding + usableWidth * normalizedHealth;

            Vector2 pos = edgeSpriteRect.anchoredPosition;
            pos.x = x;
            edgeSpriteRect.anchoredPosition = pos;
        }*/
        Debug.Log("Health material value after updating: " + _healthBarRuntimeMaterial.GetFloat(healthFillProperty) );
    }

    IEnumerator ActivateFullScreenLowHealth()
    {
        float duration = 1f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            FullScreenPlayerHit.SetFloat(controlEffect, t);

            yield return null;
        }
    }

    IEnumerator DeactivateFullScreenLowHealth()
    {
        float duration = 1f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            t = Mathf.SmoothStep(1f, 0f, t);

            FullScreenPlayerHit.SetFloat(controlEffect, t);
            Debug.Log("contrl effect value" + FullScreenPlayerHit.GetFloat(controlEffect));
            yield return null;
        }
        Debug.Log("Health value after healing: " + _hp );
    }

    public void DeactivateLowHealthFullScreen()
    {
        Debug.Log("FSS Deactivated");
        StartCoroutine(DeactivateFullScreenLowHealth());
    }

    public void StopLowHealthEffect()
    {
        FullScreenPlayerHit.SetFloat(controlEffect, 0);
    }
}