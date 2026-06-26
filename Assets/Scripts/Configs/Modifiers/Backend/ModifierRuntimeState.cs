using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.VFX;

public class ModifierRuntimeState : MonoBehaviour
{
    private readonly Dictionary<int, Dictionary<int, int>> _hitCounts = new();

    [Header("UpdateUI")]
    [SerializeField] public Image weaponBG;
    [SerializeField] public ParticleSystem weaponBGFX;
    [SerializeField] public Gradient newFireBGFXColor;
    [SerializeField] public Gradient newIceBGFXColor;
    [SerializeField] public VisualEffect fireUIEffect;
    [SerializeField] public VisualEffect iceUIEffect;
    [SerializeField] public Color fireNewColor;
    [SerializeField] public Color iceNewColor;
    [SerializeField] public float newColorDuration;
    
    [SerializeField] private float hitFreezeDuration;
    [SerializeField] private float fullEffectFreezeDuration;
    
    private readonly HashSet<int> _shotsThatAlreadyFroze = new();
    public static ModifierRuntimeState Instance { get; private set; }

    private ScriptableObject currentModifier;

    public bool isIce;

    [HideInInspector] public bool isModified;

    private Color defaultWeaponBGColor;
    private Gradient defaultBGFXGradient;
    private Coroutine uiRoutine;
    private bool defaultsCached;
    

    private class Snapshot
    {
        public bool hasMove;
        public float move;

        public bool hasFire;
        public float fireInterval;

        public bool hasRb2D;
        public RigidbodyType2D rb2DType;

        public bool hasPivot;
        public Transform weaponPivot;

        public Coroutine revertRoutine;
        public Coroutine damageRoutine;
    }

    private readonly Dictionary<int, Dictionary<int, Snapshot>> _snapshots = new();

    private void Awake()
    {
        Instance = this;
        CacheUIDefaults();
    }

    private void CacheUIDefaults()
    {
        if (defaultsCached) return;

        if (weaponBG != null)
            defaultWeaponBGColor = weaponBG.color;

        if (weaponBGFX != null)
        {
            var colorOverLifetime = weaponBGFX.colorOverLifetime;
            colorOverLifetime.enabled = true;
            defaultBGFXGradient = CopyGradient(colorOverLifetime.color.gradient);
        }

        defaultsCached = true;
    }

    public void AnimateUIToModifier(bool ice)
    {
        CacheUIDefaults();

        Color targetBGColor = ice ? iceNewColor : fireNewColor;
        Gradient targetGradient = ice ? newIceBGFXColor : newFireBGFXColor;

        AnimateUITo(targetBGColor, targetGradient);
    }

    public void AnimateUIToDefault()
    {
        CacheUIDefaults();
        AnimateUITo(defaultWeaponBGColor, defaultBGFXGradient);
    }

    private void AnimateUITo(Color targetBGColor, Gradient targetGradient)
    {
        if (uiRoutine != null)
            StopCoroutine(uiRoutine);

        uiRoutine = StartCoroutine(AnimateUICoroutine(targetBGColor, targetGradient));
    }

    private IEnumerator AnimateUICoroutine(Color targetBGColor, Gradient targetGradient)
    {
        if (weaponBG == null || weaponBGFX == null || targetGradient == null)
            yield break;

        float timer = 0f;

        Color startBGColor = weaponBG.color;

        var colorOverLifetime = weaponBGFX.colorOverLifetime;
        colorOverLifetime.enabled = true;

        Gradient startGradient = CopyGradient(colorOverLifetime.color.gradient);

        while (timer < newColorDuration)
        {
            timer += Time.deltaTime;

            float t = timer / newColorDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            weaponBG.color = Color.Lerp(startBGColor, targetBGColor, t);

            Gradient lerpedGradient = LerpGradient(startGradient, targetGradient, t);
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(lerpedGradient);

            yield return null;
        }

        weaponBG.color = targetBGColor;
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(targetGradient);
    }

    private Gradient LerpGradient(Gradient from, Gradient to, float t)
    {
        Gradient result = new Gradient();

        GradientColorKey[] fromColors = from.colorKeys;
        GradientColorKey[] toColors = to.colorKeys;

        GradientAlphaKey[] fromAlphas = from.alphaKeys;
        GradientAlphaKey[] toAlphas = to.alphaKeys;

        int colorCount = Mathf.Min(fromColors.Length, toColors.Length);
        int alphaCount = Mathf.Min(fromAlphas.Length, toAlphas.Length);

        GradientColorKey[] resultColors = new GradientColorKey[colorCount];
        GradientAlphaKey[] resultAlphas = new GradientAlphaKey[alphaCount];

        for (int i = 0; i < colorCount; i++)
        {
            resultColors[i] = new GradientColorKey(
                Color.Lerp(fromColors[i].color, toColors[i].color, t),
                Mathf.Lerp(fromColors[i].time, toColors[i].time, t)
            );
        }

        for (int i = 0; i < alphaCount; i++)
        {
            resultAlphas[i] = new GradientAlphaKey(
                Mathf.Lerp(fromAlphas[i].alpha, toAlphas[i].alpha, t),
                Mathf.Lerp(fromAlphas[i].time, toAlphas[i].time, t)
            );
        }

        result.SetKeys(resultColors, resultAlphas);
        return result;
    }

    private Gradient CopyGradient(Gradient source)
    {
        Gradient copy = new Gradient();
        copy.SetKeys(source.colorKeys, source.alphaKeys);
        return copy;
    }

    public void SetIce(bool value)
    {
        isIce = value;
    }

    public void SetModifiedState(bool value)
    {
        isModified = value;
    }

    public int IncrementHit(ScriptableObject modifier, GameObject enemy)
    {
        if (modifier == null || enemy == null) return 0;

        int modKey = modifier.GetInstanceID();
        int enemyKey = enemy.GetInstanceID();

        if (!_hitCounts.TryGetValue(modKey, out var perEnemy))
        {
            perEnemy = new Dictionary<int, int>();
            _hitCounts.Add(modKey, perEnemy);
        }

        perEnemy.TryGetValue(enemyKey, out int count);
        count++;
        perEnemy[enemyKey] = count;
        return count;
    }
    public bool TryConsumeShotFreeze(int shotId)
    {
        if (shotId <= 0)
            return true;

        if (_shotsThatAlreadyFroze.Contains(shotId))
            return false;

        _shotsThatAlreadyFroze.Add(shotId);
        StartCoroutine(ClearShotFreezeIdLater(shotId, 2f));

        return true;
    }

    private IEnumerator ClearShotFreezeIdLater(int shotId, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        _shotsThatAlreadyFroze.Remove(shotId);
    }
    
    public void ClearModifier(ScriptableObject modifier)
    {
        if (modifier == null) return;

        int modKey = modifier.GetInstanceID();

        _hitCounts.Remove(modKey);

        if (_snapshots.TryGetValue(modKey, out var perEnemy))
        {
            foreach (var kv in perEnemy)
            {
                if (kv.Value.revertRoutine != null)
                    StopCoroutine(kv.Value.revertRoutine);
            }
        }

        _snapshots.Remove(modKey);
    }

    public void ApplyTimedDebuff(
        ScriptableObject modifier,
        GameObject enemy,
        GameObject damageFX,
        float moveSpeedMul,
        float fireIntervalMul,
        float durationSeconds,
        float damage,
        bool makeBodyStatic
    )
    {
        currentModifier = modifier;
        if (modifier == null || enemy == null) return;

        int modKey = modifier.GetInstanceID();
        int enemyKey = enemy.GetInstanceID();

        if (!_snapshots.TryGetValue(modKey, out var perEnemy))
        {
            perEnemy = new Dictionary<int, Snapshot>();
            _snapshots.Add(modKey, perEnemy);
        }

        if (!perEnemy.TryGetValue(enemyKey, out var snap))
        {
            snap = new Snapshot();

            var move = enemy.GetComponentInParent<IEnemyMoveSpeed>();
            if (move != null)
            {
                snap.hasMove = true;
                snap.move = move.MoveSpeed;
            }

            var fire = enemy.GetComponentInParent<IEnemyFireInterval>();
            if (fire != null)
            {
                snap.hasFire = true;
                snap.fireInterval = fire.FireInterval;
            }

            var rb2D = enemy.GetComponentInParent<Rigidbody2D>();
            if (rb2D != null)
            {
                snap.hasRb2D = true;
                snap.rb2DType = rb2D.bodyType;
            }
           /* 
            var weaponPivot = enemy.GetComponentInParent<IWeaponPivot>();
            
            if (weaponPivot != null)
            {
                snap.hasPivot = true;
                snap.weaponPivot = weaponPivot.WeaponPivot;
            }
            */
            perEnemy.Add(enemyKey, snap);
        }

        if (snap.revertRoutine != null)
            StopCoroutine(snap.revertRoutine);

        var m = enemy.GetComponentInParent<IEnemyMoveSpeed>();
        if (snap.hasMove && m != null) m.MoveSpeed = snap.move * moveSpeedMul;

        var f = enemy.GetComponentInParent<IEnemyFireInterval>();
        if (snap.hasFire && f != null) f.FireInterval = snap.fireInterval * fireIntervalMul;

        var d = enemy.GetComponentInParent<IDamageable>();
        if (d != null && durationSeconds > 0 && damage > 0)
        {
            if (snap.damageRoutine != null)
                StopCoroutine(snap.damageRoutine);

            snap.damageRoutine = StartCoroutine(DamageOverTime(enemy, damage, damageFX, durationSeconds, modifier));
        }

        if (makeBodyStatic)
        {
            //var weaponPivot = enemy.GetComponentInParent<IWeaponPivot>();
            var rb2D = enemy.GetComponentInParent<Rigidbody2D>();
            if (snap.hasRb2D && rb2D != null)
            {
                rb2D.bodyType = RigidbodyType2D.Static;
                //weaponPivot.WeaponPivot = enemy.transform;
            }
                
        }

        snap.revertRoutine = StartCoroutine(RevertAfter(modKey, enemyKey, enemy, durationSeconds));
    }

    private IEnumerator RevertAfter(int modKey, int enemyKey, GameObject enemy, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (enemy == null) yield break;

        if (!_snapshots.TryGetValue(modKey, out var perEnemy)) yield break;
        if (!perEnemy.TryGetValue(enemyKey, out var snap)) yield break;

        var m = enemy.GetComponentInParent<IEnemyMoveSpeed>();
        if (snap.hasMove && m != null) m.MoveSpeed = snap.move;

        var f = enemy.GetComponentInParent<IEnemyFireInterval>();
        if (snap.hasFire && f != null) f.FireInterval = snap.fireInterval;

        var rb2D = enemy.GetComponentInParent<Rigidbody2D>();
        if (snap.hasRb2D && rb2D != null)
            rb2D.bodyType = snap.rb2DType;
        /*
        var weaponPivot = enemy.GetComponentInParent<IWeaponPivot>();
        if (snap.weaponPivot)
            weaponPivot.WeaponPivot = snap.weaponPivot;
            */
        if (snap.damageRoutine != null)
            StopCoroutine(snap.damageRoutine);

        perEnemy.Remove(enemyKey);
        if (perEnemy.Count == 0) _snapshots.Remove(modKey);
    }

    //enemy is enemyVisualMiddle
    private IEnumerator DamageOverTime(GameObject enemy, float damagePerSecond, GameObject damageFX, float duration, ScriptableObject modifier)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            if (enemy == null) yield break;

            var d = enemy.GetComponentInParent<IDamageable>();
            //if (d == null) Debug.LogWarning("Enemy damagable is NULL");
            //Debug.LogWarning("Damage Per Second: " + damagePerSecond);
          //  Debug.LogWarning("Duration: " + duration);

            if (d != null)
            {
                d.TakeDamage(damagePerSecond, null);
/*
                Transform enemyVisualMiddle = enemy.transform.Find("VisualCenter(DNCN)");
                Debug.LogWarning("enemy is: " + enemy.name);
                if (enemyVisualMiddle == null) Debug.LogWarning("enemyVisualMiddle is NULL");
                if (damageFX == null) Debug.LogWarning("damageFX NULL");
*/
                if ( damageFX != null)
                    Instantiate(damageFX, enemy.transform.position, Quaternion.identity);
            }
                
            yield return new WaitForSeconds(1f);
           // Debug.LogWarning("DamageOverTime delt");
        }
    }
    public IEnumerator FreezeFrame(float duration)
    {
    //    Debug.LogWarning("Freezing frame");
        Time.timeScale = 0f;
//        Debug.LogWarning("time scale:" + Time.timeScale);
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    //    Debug.LogWarning("Freezing frame Successfull");
    }

    public float HitFreezeDuration => hitFreezeDuration;
    public float FullEffectFreezeDuration => fullEffectFreezeDuration;

    public ScriptableObject Modifier => currentModifier;
}