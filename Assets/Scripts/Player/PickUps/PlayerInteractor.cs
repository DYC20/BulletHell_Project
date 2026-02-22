using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float maxPickDistance = 2.5f; // optional if you want distance filtering


    [SerializeField] private bool _IsPickInArea = false;
    [SerializeField] private Collider2D[] PickList;

    // Input System action name "Interact" -> "OnInteract"
    public void OnInteract(InputValue E)
    {
        if (_IsPickInArea)
        {
            var best = GetClosestPickable();
            Debug.Log("Trying To Pickup");
            //Debug.Log(best == null ? "No pickup candidate" : $"Trying pickup: {((MonoBehaviour)best).name} ({best.GetType().Name})");
            if (best == null) return;
            else
            {
                Debug.Log("Can be Equiped ?");
                if(best.CanPickup(gameObject))
                {
                    Debug.Log("Yes");
                    best.Pickup(gameObject);
                }
            }
            ScanForIPickups(out PickList);
            
            /*if (best != null)
            {
                bool can = best.CanPickup(gameObject);
                Debug.Log($"CanPickup={can}");
                if (can) best.Pickup(gameObject);
            }

            if (best.CanPickup(gameObject))
                best.Pickup(gameObject);*/
        }
    }


    private IPickup GetClosestPickable()
    {
        // simplest: last in range
        // better: closest; here’s closest if pickup is a MonoBehaviour
        float bestDist = float.MaxValue;
        IPickup best = null;

        foreach (var p in PickList)
        {
            IPickup mb = null;
            if(!p.gameObject.TryGetComponent<IPickup>(out mb))
            {
                Debug.Log("Not Pickable");
                continue;
            }
            float d = Vector2.Distance(transform.position, p.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = mb;
            }
            //Debug.Log("Number of candidates found:" + _candidates.Count);
        }

        return best;
    }
    private float scanTimer = 0f;

    [Header("Collecter Settings")]
    [SerializeField] private float scanInterval = 4f;
    [SerializeField] private float heightOffset = 1f;
    [SerializeField] private float detectionRadius = 1;
    [SerializeField] private LayerMask ScanLayerMask;

    private bool ScanForIPickups(out Collider2D[] locEnviroment)
    {
        Vector2 pos = Get2DPosition(transform.position) + Vector2.up * heightOffset;

        locEnviroment = Physics2D.OverlapCircleAll(pos, detectionRadius, ScanLayerMask);
        return locEnviroment.Length > 0;
    }
    void FixedUpdate()
    {
        scanTimer += Time.deltaTime;
        if (scanTimer > (1 / scanInterval))
        {
            _IsPickInArea = ScanForIPickups(out PickList);
        }
    }
    private Vector2 Get2DPosition(Vector3 Position)
    {
        Vector2 Pos = new Vector2(Position.x, Position.y);
        return Pos;
    }

    /*private void OnTriggerEnter2D(Collider2D other)
    {
        var pickup = other.GetComponentInParent<IPickup>();
        if (pickup == null) return;
        _candidates.Add(pickup);
        //Debug.Log(other.gameObject.name + " entered" + pickup.GetType().Name);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        //Debug.Log(other.gameObject.name + " eexited");
        var pickup = other.GetComponentInParent<IPickup>();
        if (pickup == null) return;
        _candidates.Remove(pickup);
    }*/
}