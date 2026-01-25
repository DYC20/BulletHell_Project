using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Fire")]
    [SerializeField] protected Transform firePoint;
    [SerializeField] protected float shotsPerSecond = 6f;

    protected GameObject owner;
    protected Teams ownerTeam;
    
    protected IAmmoConsumer ammoConsumer;

    private float _nextFireTime;
    
    public Transform FirePoint => firePoint;

    public void SetOwner(GameObject ownerGo, Teams team)
    {
        Debug.Log($"{name}.SetOwner called for {ownerGo.name}");

        owner = ownerGo;
        ownerTeam = team;
        
        ammoConsumer = owner != null ? owner.GetComponentInParent<IAmmoConsumer>() : null;
        Debug.Log($"{name}.SetOwner => owner={owner.name}, ammoConsumer={(ammoConsumer==null ? "NULL" : ammoConsumer.GetType().Name)}");
        
        if (ammoConsumer == null && owner != null)
            Debug.LogWarning($"{name}: No IAmmoConsumer found on owner parent chain. Owner={owner.name}");
    }

    public bool TryFire()
    {
        //Debug.Log("TryFire");
        if (Time.time < _nextFireTime)
            return false;

        if (firePoint == null)
        {
            Debug.LogWarning($"{name}: Missing firePoint.");
            return false;
        }

        if (!CanFire())
        {
            Debug.LogWarning($"{name}: Can't fire.");
            return false;
        }
        
        _nextFireTime = Time.time + (1f / Mathf.Max(0.01f, shotsPerSecond));
        FireInternal();
        return true;
    }

    protected virtual bool CanFire() => true;
 
    protected abstract void FireInternal();
}