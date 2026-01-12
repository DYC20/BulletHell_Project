using UnityEngine;
using UnityEngine.VFX;

public class VFXDestroy : MonoBehaviour
{
    private VisualEffect vfx;

    void Awake()
    {
        vfx = gameObject.GetComponent<VisualEffect>();
    }
    
    
    void Update()
    {
        if (!vfx.aliveParticleCount.Equals(0))
            return;

        Destroy(gameObject);
    }
}
