using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float maxPickDistance = 2.5f; // optional if you want distance filtering

    private readonly List<IPickup> _candidates = new();
    private bool _pickupPressed;

    // Input System action name "Interact" -> "OnInteract"
    public void OnInteract(InputValue E)
    {
        //Debug.Log($"OnInteract called. isPressed={E.isPressed} frame={Time.frameCount}");
        _pickupPressed = true;
    }

    private void Update()
    {
        //Debug.Log("Entered Update");
        if (!_pickupPressed) return;
        _pickupPressed = false; // treat as "press" (not hold)
        var best = GetBestCandidate();
        //Debug.Log(best == null ? "No pickup candidate" : $"Trying pickup: {((MonoBehaviour)best).name} ({best.GetType().Name})");
        if (best == null) return;
        
        if (best != null)
        {
            bool can = best.CanPickup(gameObject);
            Debug.Log($"CanPickup={can}");
            if (can) best.Pickup(gameObject);
        }

        if (best.CanPickup(gameObject))
            best.Pickup(gameObject);
    }

    private IPickup GetBestCandidate()
    {
        // simplest: last in range
        // better: closest; here’s closest if pickup is a MonoBehaviour
        float bestDist = float.MaxValue;
        IPickup best = null;

        foreach (var p in _candidates)
        {
            if (p is not MonoBehaviour mb || mb == null) continue;
            float d = Vector2.Distance(transform.position, mb.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = p;
            }
            //Debug.Log("Number of candidates found:" + _candidates.Count);
        }

        return best;
    }

    private void OnTriggerEnter2D(Collider2D other)
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
    }
}