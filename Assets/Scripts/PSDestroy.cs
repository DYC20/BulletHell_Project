using UnityEngine;

public class PSDestroy : MonoBehaviour
{
    private ParticleSystem vfx;

    void Awake()
    {
        vfx = gameObject.GetComponent<ParticleSystem>();
    }
    
    
    void Update()
    {
        if (vfx.isPlaying)
            return;

        Destroy(gameObject);
    }
}
