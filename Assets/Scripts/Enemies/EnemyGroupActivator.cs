using UnityEngine;
using System.Collections;

public class EnemyGroupActivator : MonoBehaviour
{
    [SerializeField] private float checkInterval = 0.15f;

    [SerializeField] private Camera cam;
    private EnemyChaseAI[] chasers;
    private ContactDamage[] contactDamagers;
    private Renderer[] renderers;

    private bool activated;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
        
        chasers = GetComponentsInChildren<EnemyChaseAI>(true);
        contactDamagers = GetComponentsInChildren<ContactDamage>(true);
        renderers = GetComponentsInChildren<Renderer>(true);

        // Start dormant
        foreach (var c in chasers) c.SetAIEnabled(false);
        foreach (var d in contactDamagers) d.SetDamageEnabled(false);
    }

    private void OnEnable()
    {
        StartCoroutine(VisibilityLoop());
    }

    private IEnumerator VisibilityLoop()
    {
        while (!activated)
        {
            if (IsVisibleToCamera(cam))
            {
                Activate();
                yield break;
            }
            yield return new WaitForSeconds(checkInterval);
        }
    }

    private bool IsVisibleToCamera(Camera cam)
    {
        if (cam == null) return false;

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);

        foreach (var r in renderers)
        {
            if (r == null) continue;

            // True only if this renderer's bounds intersect the MAIN camera view
            if (GeometryUtility.TestPlanesAABB(planes, r.bounds))
                return true;
        }
        return false;
    }

    public void Activate()
    {
        if (activated) return;
        activated = true;

        foreach (var c in chasers) c.SetAIEnabled(true);
        foreach (var d in contactDamagers) d.SetDamageEnabled(true);
    }
}
